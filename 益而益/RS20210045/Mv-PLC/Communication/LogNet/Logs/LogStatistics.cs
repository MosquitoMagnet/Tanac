using Communication.Core;
using Communication.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Communication.LogNet
{
	/// <summary>
	/// 一个统计次数的辅助类，可用于实现分析一些次数统计信息，比如统计某个API最近每天的访问次数，
	/// 统计日志组件最近每天访问的次数，调用者只需要关心统计方式和数据个数，详细参照API文档。<br />
	/// An auxiliary class for counting the number of times, which can be used to realize the analysis of some number of times statistical information, 
	/// such as counting the number of daily visits of an API, and counting the number of daily visits of the log component. 
	/// The caller only needs to care about the statistical method and the number of data. Refer to details API documentation.
	/// </summary>
	/// <example>
	/// 我们来举个例子：我有个方法，AAA需要记录一下连续60天的调用次数信息
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\LogNet\LogStatisticsSample.cs" region="Sample1" title="简单的记录调用次数" />
	/// 因为这个数据是保存在内存里的，程序重新运行就丢失了，如果希望让这个数据一直在程序的话，在软件退出的时候需要存储文件，在软件启动的时候，加载文件数据
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\LogNet\LogStatisticsSample.cs" region="Sample2" title="存储与加载" />
	/// </example>
	public class LogStatistics : LogStatisticsBase<long>
	{
		/// <inheritdoc cref="LogStatisticsBase{T}.LogStatisticsBase(GenerateMode, int)"/>
		public LogStatistics(GenerateMode generateMode, int dataCount) : base(generateMode, dataCount)
		{
			this.byteTransform = new RegularByteTransform();
		}

		/// <summary>
		/// 新增一个统计信息，将会根据当前的时间来决定插入数据位置，如果数据位置发生了变化，则数据向左发送移动。如果没有移动或是移动完成后，最后一个数进行新增数据 frequency 次<br />
		/// Adding a new statistical information will determine the position to insert the data according to the current time. If the data position changes, 
		/// the data will be sent to the left. If there is no movement or after the movement is completed, add data to the last number frequency times
		/// </summary>
		/// <param name="frequency">新增的次数信息，默认为1</param>
		[HslMqttApi]
		public void StatisticsAdd(long frequency = 1)
		{
			StatisticsCustomAction(m => m + frequency);
		}

		/// <summary>
		/// 将当前的统计信息及数据内容写入到指定的文件里面，需要指定文件的路径名称<br />
		/// Write the current statistical information and data content to the specified file, you need to specify the path name of the file
		/// </summary>
		/// <param name="fileName">文件的完整的路径名称</param>
		public void SaveToFile(string fileName)
		{
			OperateResult<long, long[]> dataSnap = GetStatisticsSnapAndDataMark();
			byte[] buffer = new byte[(dataSnap.Content2.Length + 1) * 8];
			byteTransform.TransByte(dataSnap.Content1).CopyTo(buffer, 0);
			byteTransform.TransByte(dataSnap.Content2).CopyTo(buffer, 8);
			File.WriteAllBytes(fileName, buffer);
		}

		/// <summary>
		/// 从指定的文件加载对应的统计信息，通常是调用<see cref="SaveToFile(string)"/>方法存储的文件，如果文件不存在，将会跳过加载<br />
		/// Load the corresponding statistical information from the specified file, usually the file stored by calling the <see cref="SaveToFile(string)"/> method. 
		/// If the file does not exist, the loading will be skipped
		/// </summary>
		/// <param name="fileName">文件的完整的路径名称</param>
		public void LoadFromFile(string fileName)
		{
			if (File.Exists(fileName))
			{
				byte[] buffer = File.ReadAllBytes(fileName);
				long dataMark = byteTransform.TransInt64(buffer, 0);
				long[] temp = byteTransform.TransInt64(buffer, 8, (buffer.Length - 8) / 8);
				Reset(temp, dataMark);
			}
		}

		/// <inheritdoc/>
		public override string ToString() => $"LogStatistics[{GenerateMode}:{DataCount}]";



		private RegularByteTransform byteTransform;                      // 数据转换的对象
	}
}
