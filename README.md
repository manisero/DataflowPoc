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
  - no difference observed :(
  - probably should try on more records
    - currently: 100k records, 10k batch size
    - to try: 1kk records, 10k batch size
- Test for different processor cores number
  - doubled cores, halved time
- More performance tracking
  - memory consumption (performance counters)
    - Dataflow enables control over memory consumption
  - garbage collections (ETW)
    - pooling did not lower GCs rate (see 'Object pooling')

To do
---
