-- ========================================================================== --
--                                                                            --
--  NOTE                                                                      --
--    On Windows:                                                             --
--      compile with:                                                         --
--        `ghc --make valuez.worker.hs`                                       --
--      or                                                                    --
--      use interactively:                                                    --
--        `ghci values.worker.hs`                                             --
--        at the prompt type `main`                                           --
--                                                                            --
-- ========================================================================== --
{-# LANGUAGE OverloadedStrings #-}

module Main where

import System.ZMQ4
import Data.ByteString.Char8 (unpack, empty)
import System.IO (hSetBuffering, stdout, BufferMode(..))
import Control.Concurrent (threadDelay)
import Control.Applicative ((<$>))
import Control.Monad (when, unless)

-- # get input data from source
-- # performs calculation
--   PULL tcp://localhost:9004
-- # sends "ready" signal to reducer
-- # sends calculated result to reducer
--   PUSH tcp://localhost:9005
-- # receives control signal from reducer
--   SUB  tcp://localhost:9006
--     ?> [ command :EXIT = 1uy ]
main :: IO ()
main =
    -- create upstream, downstream connections
    withContext $ \ctx ->
        withSocket ctx Pull $ \receiver   ->
        withSocket ctx Push $ \sender     ->
        withSocket ctx Sub  $ \controller -> do
            -- socket to receive commands from upstream
            connect receiver "tcp://localhost:9004"
            -- socket to send results downstream
            connect sender "tcp://localhost:9005"
            -- socket to await kill signal broadcast
            connect controller "tcp://localhost:9006"
            subscribe controller "" -- get broadcast for all topics

            -- set up infinite polling loop
            hSetBuffering stdout NoBuffering
            pollContinuously receiver sender controller
    where
    pollContinuously ::  (Receiver r, Sender s) => Socket r -> Socket s -> Socket c -> IO ()
    pollContinuously sock_recv sock_to_send ctr  = do

        [a, b] <- poll (-1) [Sock sock_recv [In] Nothing, Sock ctr [In] Nothing]

        when (In `elem` a) $ do
            msg <- unpack <$> receive sock_recv
            -- Simple progress indicator for the viewer
            putStr $ msg ++ "."
            -- Do the "work"
            threadDelay (read msg * 1000)
            -- Send results to sink
            send sock_to_send [] empty

        unless (In `elem` b) $
             pollContinuously sock_recv sock_to_send ctr
