using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Communication.Reflection
{
	/// <summary>
	/// 可以指定方法变成对外公开的API接口，如果方法不实现该特性，将不对外公开方法，无法获取相关的接口权限<br />
	/// You can specify the method to become an externally public API interface. If the method does not implement this feature, 
	/// the method will not be publicly disclosed, and the related interface permissions cannot be obtained
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class HslMqttApiAttribute : Attribute
	{
		/// <summary>
		/// 当前指定的ApiTopic信息，如果当前的方法接口不指定别名，那么就使用当前的方法名称<br />
		/// The currently specified ApiTopic information, if the current method interface does not specify an alias, 
		/// then the current method name is used
		/// </summary>
		public string ApiTopic { get; set; }

		/// <summary>
		/// 当前方法的注释内容<br />
		/// The comment content of the current method
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// 指定描述内容来实例化一个的对象
		/// </summary>
		/// <param name="description">当前接口的描述信息</param>
		public HslMqttApiAttribute(string description)
		{
			this.Description = description;
		}

		/// <summary>
		/// 指定描述内容来实例化一个的对象
		/// </summary>
		/// <param name="apiTopic">指重新定当前接口的路由信息</param>
		/// <param name="description">当前接口的描述信息</param>
		public HslMqttApiAttribute(string apiTopic, string description)
		{
			this.ApiTopic = apiTopic;
			this.Description = description;
		}

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public HslMqttApiAttribute() { }
	}
}
