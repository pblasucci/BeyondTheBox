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

# setup a simple portfolio
stocks = [ (b"MSFT",  41.78)
         , (b"AAPL",  95.35)
         , (b"GOOG", 571.09)
         , (b"YHOO",  34.53)
         , (b"BBRY",  10.90) ]

random.seed()
# helper function to create a randomly values trade (given the portfolio)
# NOTE: all orders are assumed to have a size of 1 lot
def order ():
  # pick a security
  (stock,base) = stocks[random.randint(0, 4)]
  # pick an action
  buySell = random.choice([1, -1])
  # set desired share price
  price = base + random.uniform(-3.0,3.0)
  # package order for transmission
  return  [ stock
          , str(buySell).encode('UTF8')
          , str(price  ).encode('UTF8') ]

context = zmq.Context()
# socket by which orders are sent to worker(s)
source = context.socket(zmq.PUSH)
source.bind("tcp://*:9004")
# direct connection to aggregator
reduce = context.socket(zmq.PUSH)
reduce.connect("tcp://localhost:9005")

# wait for worker coordination
_ = input("Press Enter when the workers are ready ")
print("Sending tasks to worker...")

# signal aggregator that batch has started
reduce.send(b'0')

# send out a batch of orders for pricing
for _ in range(100):
  source.send_multipart(order())

# give ZMQ a chance to deliver
time.sleep(1)
