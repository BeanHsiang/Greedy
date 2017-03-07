using Greedy.Dapper.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Dapper
{
    class Snowflake
    {
        private static readonly DateTime OriginDate = new DateTime(1970, 1, 1).ToUniversalTime();

        //开始该类生成ID的时间截，1288834974657 (Thu, 04 Nov 2010 01:42:54 GMT) 这一时刻到当前时间所经过的毫秒数，占 41 位（还有一位是符号位，永远为 0）。  
        private const long START_TIME = 1463834116272L;

        //机器id所占的位数
        private const int WORKER_ID_BITS = 5;

        //数据标识id所占的位数  
        private const int DATACENTER_ID_BITS = 5;

        //支持的最大机器id，结果是31,这个移位算法可以很快的计算出几位二进制数所能表示的最大十进制数（不信的话可以自己算一下，记住，计算机中存储一个数都是存储的补码，结果是负数要从补码得到原码）
        private long maxWorkerId = -1L ^ (-1L << WORKER_ID_BITS);

        //支持的最大数据标识id  
        private long maxDatacenterId = -1L ^ (-1L << DATACENTER_ID_BITS);

        //序列在id中占的位数  
        private const int SEQUENCE_BITS = 12;

        //机器id向左移12位
        private const int WORKER_ID_LEFT_SHIFT = SEQUENCE_BITS;

        //数据标识id向左移17位 
        private const int DATACENTER_ID_LEFT_SHIFT = WORKER_ID_BITS + WORKER_ID_LEFT_SHIFT;

        //时间截向左移5+5+12=22位
        private const int TIMESTAMP_LEFT_SHIFT = DATACENTER_ID_BITS + DATACENTER_ID_LEFT_SHIFT;

        //生成序列的掩码，这里为1111 1111 1111
        private long sequenceMask = -1 ^ (-1 << SEQUENCE_BITS);

        private long workerId;

        private long datacenterId;

        //同一个时间截内生成的序列数，初始值是0，从0开始  
        private long sequence = 0L;

        //上次生成id的时间截  
        private long lastTimestamp = -1L;

        public Snowflake()
        {
            try
            {
                var section = ConfigurationManager.GetSection("greedydapper") as GreedyDapperSection;
                this.workerId = section.Snowflake.WorkerId;
                this.datacenterId = section.Snowflake.DataCenterId;
            }
            catch (ConfigurationErrorsException ex)
            {
                throw ex;
            }
        }

        public Snowflake(long workerId, long datacenterId)
        {
            if (workerId < 0 || workerId > maxWorkerId)
            {
                throw new ArgumentException(string.Format("workerId{0:d} is less than 0 or greater than maxWorkerId{1:d}.", workerId, maxWorkerId));
            }
            if (datacenterId < 0 || datacenterId > maxDatacenterId)
            {
                throw new ArgumentException(string.Format("datacenterId{0:d} is less than 0 or greater than maxDatacenterId{1:d}.", datacenterId, maxDatacenterId));
            }
            this.workerId = workerId;
            this.datacenterId = datacenterId;
        }

        public long GenerateId()
        {
            long timestamp = GenerateTime();
            if (timestamp < lastTimestamp)
            {
                throw new Exception(string.Format("Clock moved backwards.  Refusing to generate id for {0:d} milliseconds", lastTimestamp - timestamp));
            }
            //如果是同一时间生成的，则自增
            if (timestamp == lastTimestamp)
            {
                sequence = (sequence + 1) & sequenceMask;
                if (sequence == 0)
                {
                    //生成下一个毫秒级的序列  
                    timestamp = TilNextMillis();
                    //序列从0开始  
                    sequence = 0L;
                }
            }
            else
            {
                //如果发现是下一个时间单位，则自增序列回0，重新自增 
                sequence = 0L;
            }

            lastTimestamp = timestamp;

            //看本文第二部分的结构图，移位并通过或运算拼到一起组成64位的ID
            return ((timestamp - START_TIME) << TIMESTAMP_LEFT_SHIFT)
                | (datacenterId << DATACENTER_ID_LEFT_SHIFT)
                | (workerId << WORKER_ID_LEFT_SHIFT)
                | sequence;
        }

        protected long GenerateTime()
        {
            return Convert.ToInt64(DateTime.Now.ToUniversalTime().Subtract(OriginDate).TotalMilliseconds);
        }

        protected long TilNextMillis()
        {
            long timestamp = GenerateTime();
            if (timestamp <= lastTimestamp)
            {
                timestamp = GenerateTime();
            }
            return timestamp;
        }


    }
}
