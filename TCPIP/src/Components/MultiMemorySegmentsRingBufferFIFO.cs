namespace TCPIP
{
    // Implements a ringbus model, with segments saved in a ring like structure
    // This model guarantees a fifo queue
    public class MultiMemorySegmentsRingBufferFIFO<MetaData> : IMultiMemorySegments<MetaData> where MetaData : struct
    {
        public struct SegmentEntry{
            public int start; // Start address. Include byte
            public int stop; // Stop address, Exclude last byte
            public int current; // The current byte being written
            public bool done; // The segment has been fully read
            public bool full; // The segment is full
            public MetaData metaData; // The meta data information
        }
        readonly int num_segments;
        readonly int memory_size;

        // Pointers to detect head and tail of the ringbuffer
        private int next_head_segment_id = 0; // This points to the next free head segment
        private int current_tail_segment_id = 0; // This points to the current tail segment
        private SegmentEntry[] segment_list;



        ////////// interface
        public MultiMemorySegmentsRingBufferFIFO(int num_segments,int memory_size)
        {
            this.num_segments = num_segments;
            this.memory_size = memory_size;
            // Allocate the ring entry segments
            this.segment_list = new SegmentEntry[num_segments];
            // Set default values
            for (int i = 0; i < num_segments; i++)
            {
                SegmentEntry x = segment_list[i];
                x.done = true;
                x.full = true;
                x.start = 0;
                x.stop = 0;
                x.current = 0;
                segment_list[i] = x;
            }
        }

        public int SegmentsLeft()
        {
            int ret = 0;
            // Iterate over the ringbus and accumulate all done RingEntry's
            for (int i = 0; i < num_segments; i++)
            {
                ret += segment_list[i].done && segment_list[i].full  ? 1 : 0;
            }
            return ret;
        }

        public int SaveData(int segment_ID, int offset)
        {
            // Get the current segment
            SegmentEntry cur_segment = segment_list[segment_ID];
            if (cur_segment.full){
                throw new System.Exception($"Segment ID:{segment_ID} is marked as full, so data cannot be saved");
            }
            return (cur_segment.start + offset) % memory_size;
        }

        public int SaveData(int segment_ID)
        {
            // Get the current segment
            SegmentEntry cur_segment = segment_list[segment_ID];
            if (cur_segment.full){
                throw new System.Exception($"Segment ID:{segment_ID} is marked as full, so data cannot be saved");
            }

            // Calculate the offset
            int ret = (cur_segment.start + cur_segment.current) % memory_size;

            // Increment the current counter
            cur_segment.current++;

            // If the next byte is the last, we mark the segment as full, and reset counters
            if ((cur_segment.start + cur_segment.current) % memory_size == cur_segment.stop){
                Logging.log.Info($"Segment:{segment_ID} is fully saved based on counter, Marking it full");
                SegmentFull(segment_ID);
                cur_segment = segment_list[segment_ID];
                cur_segment.current = 0;

            }

            // Put the data back
            segment_list[segment_ID] = cur_segment;
            return ret;
        }

        public int LoadData(int segment_ID, int offset = -1)
        {
            // Get the current segment
            SegmentEntry cur_segment = segment_list[segment_ID];
            if (cur_segment.done || !cur_segment.full){
                // throw new System.Exception($@"Segment ID:{segment_ID}
                //                               should be Done=False,
                //                               but is Full={cur_segment.full}, Done={cur_segment.done}.
                //                               Data cannot be loaded");
                return -1;
            }
            return (cur_segment.start + offset) % memory_size;
        }

        public int LoadData(int segment_ID)
        {
            // Get the current segment
            SegmentEntry cur_segment = segment_list[segment_ID];
            if (cur_segment.done || !cur_segment.full){
                // throw new System.Exception($@"Segment ID:{segment_ID}
                //                               should be Done=False,
                //                               but is Full={cur_segment.full}, Done={cur_segment.done}.
                //                               Data cannot be loaded");
                return -1;
            }
            // Calculate the offset
            int ret = (cur_segment.start + cur_segment.current) % memory_size;
            cur_segment.current++;
            // If the next byte is the last, we mark the segment as full, and reset counters
            if (cur_segment.current % memory_size == cur_segment.stop){
                Logging.log.Info($"Segment:{segment_ID} is fully loaded based on counter, Marking it done");
                SegmentDone(segment_ID);
                cur_segment = segment_list[segment_ID];
                cur_segment.current = 0;
            }
            // Put the data back
            segment_list[segment_ID] = cur_segment;
            return ret;
        }

        public int AllocateSegment(int size)
        {
            int last_segment_id = next_head_segment_id - 1 < 0 ? num_segments -1 : next_head_segment_id - 1;
            int new_segment_id = next_head_segment_id;
            // int last_segment_id = head_segment_id;
            // int new_segment_id = (last_segment_id + 1) % num_segments;
            SegmentEntry last_segment = segment_list[last_segment_id];
            SegmentEntry new_segment = segment_list[new_segment_id];
            SegmentEntry tail_segment = segment_list[current_tail_segment_id];

            // If the next segment is done or full, it is not ready
            if(!new_segment.done && !new_segment.full)
            {
               throw new System.Exception("The segment entry table is full!");
            }
            // If the range is currently bigger than what we can handle, there is nothing to do
            // If the tail segment and the last segment is are the same, then we must be hitting
            // themselves (full empty buffer)
            if (MemoryRange(last_segment.stop, tail_segment.start) < size && current_tail_segment_id != new_segment_id){
                Logging.log.Error($"The range : {last_segment.stop},{tail_segment.start} is not large enough for {size}");
                throw new System.Exception("The range is not big enough for the allocation");
            }
            new_segment.done = false;
            new_segment.full = false;
            new_segment.start = last_segment.stop;
            // Offset with last byte, so we do not have to subtract 1
            new_segment.stop = (new_segment.start + size) % memory_size;
            new_segment.current = 0;
            // save the segment
            segment_list[new_segment_id] = new_segment;
            // set the head segment to a new segment
            next_head_segment_id = (new_segment_id + 1) % num_segments;
            return new_segment_id;
        }

        public bool IsSegmentDone(int segment_ID)
        {
            return segment_list[segment_ID].done;
        }

        public bool IsSegmentFull(int segment_ID)
        {
            return segment_list[segment_ID].full;
        }

        public int SegmentBytesLeft(int segment_ID)
        {
            SegmentEntry cur_segment = segment_list[segment_ID];
            return MemoryRange(cur_segment.current, cur_segment.stop);
        }

        public void SegmentDone(int segment_ID)
        {
            Logging.log.Info($"Marking Segment_ID:{segment_ID} done");
            SegmentEntry cur_segment = segment_list[segment_ID];
            if (!cur_segment.full){
                throw new System.Exception($@"Segment ID:{segment_ID} Cannot mark as done, when it is not full first");
            }
            cur_segment.done = true;
            segment_list[segment_ID] = cur_segment;
            // See if we should progress the tail pointer;
            for(int i = 1; i < num_segments; i++)
            {
                int scope_pointer = (i + current_tail_segment_id) % num_segments;
                cur_segment = segment_list[scope_pointer];
                current_tail_segment_id = (current_tail_segment_id + i) % num_segments;
                if(cur_segment.full)
                {
                    return;
                }
            }
        }

        public void SegmentFull(int segment_ID)
        {
            Logging.log.Info($"Marking Segment_ID:{segment_ID} full");
            SegmentEntry cur_segment = segment_list[segment_ID];
            cur_segment.full = true;
            segment_list[segment_ID] = cur_segment;
        }

        public void SegmentRollback(int segment_ID,int count = 1){
           SegmentEntry cur_segment = segment_list[segment_ID];
            cur_segment.current -= count;
            segment_list[segment_ID] = cur_segment;
        }

        public void SaveMetaData(int segment_ID, MetaData meta_data)
        {
            SegmentEntry cur_segment = segment_list[segment_ID];
            cur_segment.metaData = meta_data;
            segment_list[segment_ID] = cur_segment;
            //segment_list[segment_ID].metaData = meta_data;

        }

        public MetaData LoadMetaData(int segment_ID)
        {
            return segment_list[segment_ID].metaData;
        }

        public int FocusSegment()
        {
            return current_tail_segment_id;
        }

        /////// Helper functions
        // Get amount of memory between range
        private int MemoryRange(int start, int stop){
            // if the start is higher than the stop, it must wrap around
            if (start > stop){
                return memory_size - (start - stop);
            } else{
                return stop - start;
            }
        }
    }
}
