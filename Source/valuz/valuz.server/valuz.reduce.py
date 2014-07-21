import sys
import time
import zmq

context = zmq.Context()

# Socket to receive messages on
receiver = context.socket(zmq.PULL)
receiver.bind("tcp://*:9005")

# Socket for worker control
controller = context.socket(zmq.PUB)
controller.bind("tcp://*:9006")

# Wait for start of batch
receiver.recv()

# Start our clock now
tstart = time.time()

# Process 100 confirmiations
for task_nbr in range(100):
    receiver.recv()
    if task_nbr % 10 == 0:
        sys.stdout.write(":")
    else:
        sys.stdout.write(".")
    sys.stdout.flush()

# Calculate and report duration of batch
tend = time.time()
tdiff = tend - tstart
total_msec = tdiff * 1000
print("Total elapsed time: %d msec" % total_msec)

# Send kill signal to workers
controller.send(b"KILL")

# Finished
time.sleep(1)  # Give 0MQ time to deliver
