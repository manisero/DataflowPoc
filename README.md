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

To do
---

- More performance tracking
  - memory consumption (performance counters)
  - garbage collections (ETW)
- Test for different processor cores number
