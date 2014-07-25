-- ========================================================================== --
--                                                                            --
--  NOTE                                                                      --
--    On Windows:                                                             --
--      compile with:                                                         --
--        `ghc --make valuz.worker.hs`                                       --
--      or                                                                    --
--      use interactively:                                                    --
--        `ghci valuz.worker.hs`                                             --
--        at the prompt type `main`                                           --
--                                                                            --
-- ========================================================================== --
{-# LANGUAGE OverloadedStrings #-}

module Main where

import Control.Concurrent (threadDelay)
import Control.Monad (when, unless)
import Data.ByteString.Char8 (unpack, pack, empty)
import System.IO (hSetBuffering, stdout, BufferMode(..))
import System.Random
import System.ZMQ4

-- # gets input data from source
--   PULL tcp://localhost:9004
--   -> [ stock  : UTF8([A-Z][A-Z0-9]+)       ]
--      [ action : UTF8(BUY = +1 | SELL = -1) ]
--      [ price  : UTF8(f64)                  ]
-- # sends calculated result to reducer
--   PUSH tcp://localhost:9005
--   <- [ value : UTF8(f64) ]
-- # receives control signal from reducer
--   SUB  tcp://localhost:9006
--   ?> [ UTF8('batch.leave') ]

main :: IO ()
main =
  -- create upstream, downstream connections
  withContext $ \ctx ->
    withSocket ctx Pull $ \source ->
    withSocket ctx Push $ \reduce ->
    withSocket ctx Sub  $ \cancel -> do
      -- socket to receive commands from upstream
      connect source "tcp://localhost:9004"
      -- socket to send results downstream
      connect reduce "tcp://localhost:9005"
      -- socket to await kill signal broadcast
      connect   cancel "tcp://localhost:9006"
      subscribe cancel "batch.leave" -- listen for shutdown signal

      -- indicate readiness to begin work
      send reduce [] empty

      -- set up infinite polling loop
      hSetBuffering stdout NoBuffering
      pollContinuously source reduce cancel
  where
  pollContinuously ::  (Receiver r, Sender s) => Socket r -> Socket s -> Socket c -> IO ()
  pollContinuously source reduce cancel = do
    -- set up socket polling callbacks
    [order, abort] <- poll (-1) [ Sock source [In] Nothing
                                , Sock cancel [In] Nothing ]

    -- if order was sent, do actual work
    when (In `elem` order) $ do
      msg <- receiveMulti source
      let stock   =       unpack (msg !! 0)
      let buySell = read (unpack (msg !! 1)) :: Float
      let price   = read (unpack (msg !! 2)) :: Float
      let value   = buySell * price * 100 -- fixed size, 1 lot
      -- simulate complex "work"
      pause <- getStdRandom $ randomR (1, 100)
      threadDelay pause
      -- send result to aggregator
      send reduce [] (pack (show value))

    -- if shutdown signal not sent, poll again
    unless (In `elem` abort) $
       pollContinuously source reduce cancel
