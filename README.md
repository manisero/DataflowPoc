DataflowPoc
---

.NET TPL Dataflow proof of concept.

Done
---

- Unified way of blocks creation
  - DataflowFacade
  - StartableBlock
  - ProcessingBlock
- Block post-processing handling
  - Completion.ContinueWithStatusPropagation
- Block task and post-processing task status propagation
  - Completion.ContinueWithStatusPropagation
- Progress reporting
  - ProgressReportingBlockFactory
- Global completion
  - TaskExtensions.CreateGlobalCompletion
- Cancellation
  - CancellationToken passing
- Unified error propagation
  - Exception always available at output block's completion task
  - StartableBlock.Start exception faults it's Output
- Backward error propagation
  - TaskExtensions.CreateGlobalCompletion
- Railroad pipeline
  - RailroadPipelineFactory
- Straight pipeline
  - StraightPipelineFactory
- Performance tracking (ETW)
  - Events.Write
- Performance analysis
  - PerfAnalyzer project
    - parses results from PerfView
  - Google Sheets with Gantt diagram
    - https://docs.google.com/spreadsheets/d/1RHPZqPYo1qNIN3ml-zwsiMH5Dq-NRGdvSSOZMlMN6qs/edit?usp=sharing
  - easy to spot bottlenecks
- Object pooling
  - DataPool
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
- Implement real-life scenario
  - exporting data from database to csv

To do
---

- ProgressReportingBlock with dynamic EstimatedOutputCount setting
 - not while creating the block, e.g. inside StartableBlock.Start action
