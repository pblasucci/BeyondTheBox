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
combine = context.socket(zmq.PULL)
combine.bind("tcp://*:9005")
goodbye = context.socket(zmq.PUB)
goodbye.bind("tcp://*:9006")

combine.recv()

tstart = time.time()

value = 0
for task in range(100):
    value += float(combine.recv().decode('UTF8'))
    if task % 10 == 0:
        sys.stdout.write(":")
    else:
        sys.stdout.write(".")
    sys.stdout.flush()

tend = time.time()
tdiff = tend - tstart
total = tdiff * 1000
print("Total elapsed time: %d msec" % total)
print("Value: %d" % value)

goodbye.send(b'0')

time.sleep(1)

_ = input("Press <RETURN> to exit")
