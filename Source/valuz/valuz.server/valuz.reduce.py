import struct
import sys
import time
import zmq

# ======================================================================
# # waits for worker(s) to signal readiness
# # gets results of individual calculation from worker(s)
#   PULL tcp://*:9005
#   -> []
#   -> [ value : UTF8(f64) ]
# # sends control signal to source
# # sends control signal to worker(s)
#   PUB tcp://*:9006
#   << [ UTF8('batch.start') ]
#   << [ UTF8('batch.leave') ]
# ======================================================================

context = zmq.Context()
# socket for receiving data, signals
collect = context.socket(zmq.PULL)
collect.bind("tcp://*:9005")
# socket by which workers are signalled
control = context.socket(zmq.PUB)
control.bind("tcp://*:9006")

# wait for all workers to report readiness
expected = int(sys.argv[1])
capacity = 0
while capacity < expected :
  _ = collect.recv()
  # NOTE: just wait (forever) for all workers.
  #       a more sophisticated app my handle this differently
  capacity += 1

# tell distributor to initiate batch
time.sleep(1)
control.send(b'batch.start')
print("Starting batch job")

# observe when the batch started
tstart = time.time()

value = 0
for task in range(100):
  # accumulate results of individual calculations
  value += float(collect.recv().decode('UTF8'))
  # simple progress report
  if task % 10 == 0:
    sys.stdout.write(":")
  else:
    sys.stdout.write(".")
  sys.stdout.flush()

# observe when the batch finished
tend = time.time()

# display elapsed batch time
tdiff = tend - tstart
total = tdiff * 1000
print("\nTotal elapsed time: %d msec" % total)

# display final calculated result
print("Calculated result: {:+.3f}".format(value))

# tell workers to shutdown
control.send(b'batch.leave')

# give user a chance to review the resutls
_ = input("Press <RETURN> to exit")
