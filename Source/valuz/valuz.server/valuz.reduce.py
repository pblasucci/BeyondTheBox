import struct
import sys
import time
import zmq

## gets results of individual calculation from worker(s)
#  PULL tcp://*:9005
#  -> [ command : READY = 0uy ] # from source, to start batch
#  -> [ value   : UTF8 => f64 ] # from worker(s), for processing
## sends control signals to source, worker(s)
#  PUB tcp://*:9006
#  << [ comand : EXIT = 0uy ] # to worker(s), after completion
## displays aggregate calculation

context = zmq.Context()
# socket for receiving data, signals
combine = context.socket(zmq.PULL)
combine.bind("tcp://*:9005")
# socket by which workers are signalled
goodbye = context.socket(zmq.PUB)
goodbye.bind("tcp://*:9006")

# wait for START (of batch) signal
combine.recv()
# note when the batch started
tstart = time.time()

value = 0
for task in range(100):
  # accumulate results of individual calculations
  value += float(combine.recv().decode('UTF8'))
  # simple progress report
  if task % 10 == 0:
    sys.stdout.write(":")
  else:
    sys.stdout.write(".")
  sys.stdout.flush()

# note when the batch finished
tend = time.time()

# display elapsed batch time
tdiff = tend - tstart
total = tdiff * 1000
print("Total elapsed time: %d msec" % total)

# display final calculated result
print("Value: %d" % value)

# tell workers to shutdown
goodbye.send(b'0')

# give user a chance to review the resutls
_ = input("Press <RETURN> to exit")
