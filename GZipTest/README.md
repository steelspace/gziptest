# GZipTest

## Usage
Since this is a .NET Core 2.2 app, you need an SDK, VS and the you can run the app from console with:
```
dotnet GZipTest.dll compress|decompress source-file target-path
```
Existing files are automatically rewritten.

## Concept
Since it wasn't possible to utilize standard .net tasks I implemented very simple version of producer-consumer tasking/thread pooling where I can add as much as task as there is processor cores.
They are mutally synchronized using mutex and lock().
I have been already implementing something similar in the past so I just needed to refresh my idea of tasks where

## Logical blocks

### OperationCommand
Handles command line parameters

### Compressor
High-level utility which manages the compression and decompression

### /Tasks
Contains Task, Buffer (producer-consumer) and Queue of threads/tasks for compression/decompression and writing
- Queue has dedicated threads to provide tasks by dequeing them from the queues (there is a queue for writing and another one for compression/decompression)

### /Zipper
Contains code actual compression and decompression
- Chunk represent a part of the file to be compressed/decompressed with Gzip
- Zipper manages chunks in files, adds tasks into queue

## To Do
- logging
- better error handling
- unit tests (challenging :)

## Remarks
- this is really challenging task, but I hope I managed to write it in a safe way.
- I used exception throwing to interrupt the programme and display error mesasges to the console
- I would strongly advise using standard .NET task and async in real-life scenarios
- I can see some possibilities where lock() could not be probably necessary but it is very hard to test such scenario for real

## Results
- 10GB special comrepssion test file
	- This app: 1 minute 18 seconds
	- 7-Zip: 8 minutes
- In both cases the CPU utilization was 100%
- The compressed size was almost the same (+- 1%%)
