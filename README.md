DataflowPoc
---

.NET TPL Dataflow proof of concept.

Done
---

- Unified way of blocks creation
- Block post-processing handling
- Block task and post-processing task status propagation
- Progress reporting
- Global completion
- Cancellation
- Backward error propagation
- Railroad pipeline
- Performance tracking (ETW)
- Performance analysis (Gantt)
  - easy to spot bottlenecks
- Object pooling
  - tested:
    - 100k records, 10k batch size
    - 1kk records, 10k batch size
  - no difference observed :(
- Test for different processor cores number
  - doubled cores, halved time
- More performance tracking
  - memory consumption (performance counters)
    - Dataflow enables control over memory consumption
  - garbage collections (ETW)
    - pooling did not lower GCs rate (see 'Object pooling')
- Tested MaxMessagesPerTask
  - no difference observed :(
- Tested SingleProducerConstrained
  - no difference observed :(

To do
---

- Implement real-life scenario
  - exporting data from database to csv
