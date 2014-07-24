import random
import time
import zmq

## sends batch data to worker(s) for processing
#  PUSH tcp://*:9004
#  <- [ stock  : UTF8([A-Z][A-Z0-9]+)          ]
#     [ action : (BUY = 1 | SELL = -1) => UTF8 ]
#     [ price  : f64 => UTF8                   ]
## sends "start of batch" signal to reducer
#  PUSH tcp://localhost:9006
#  <- [ command : START = 0uy ]

stocks = [ (b"MSFT",  41.78)
         , (b"AAPL",  95.35)
         , (b"GOOG", 571.09)
         , (b"YHOO",  34.53)
         , (b"BBRY",  10.90) ]

random.seed()

def order ():
  (stock,base) = stocks[random.randint(0, 4)]
  buySell      = random.choice([1, -1])
  price        = base + random.uniform(-3.0,3.0)
  return  [ stock
          , str(buySell).encode('UTF8')
          , str(price  ).encode('UTF8') ]


context = zmq.Context()
source = context.socket(zmq.PUSH)
source.bind("tcp://*:9004")
reduce = context.socket(zmq.PUSH)
reduce.connect("tcp://localhost:9005")

_ = input("Press Enter when the workers are ready ")
print("Sending tasks to worker...")

reduce.send(b'0')

for _ in range(100):
  source.send_multipart(order())

time.sleep(1)

_ = input("Press <RETURN> to exit ")
