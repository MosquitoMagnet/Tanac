using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using Communication.LogNet;
using System.Net.Http;
using System.Threading.Tasks;

namespace Communication.Core.Net
{
	/// <summary>
	/// 基于webapi的数据访问的基类，提供了基本的http接口的交互功能<br />
	/// A base class for data access based on webapi that provides basic HTTP interface interaction
	/// </summary>
	/// <remarks>
	/// 当前的基类在.net framework上存在问题，在.net framework4.5及.net standard上运行稳定而且正常
	/// </remarks>
	public class NetworkWebApiBase
	{
		#region Constrcutor

		/// <summary>
		/// 使用指定的ip地址来初始化对象<br />
		/// Initializes the object using the specified IP address
		/// </summary>
		/// <param name="ipAddress">Ip地址信息</param>
		public NetworkWebApiBase(string ipAddress)
		{
			this.ipAddress = ipAddress;
#if !NET35 && !NET20
			this.httpClient = new HttpClient();
#endif
		}

		/// <summary>
		/// 使用指定的ip地址及端口号来初始化对象<br />
		/// Initializes the object with the specified IP address and port number
		/// </summary>
		/// <param name="ipAddress">Ip地址信息</param>
		/// <param name="port">端口号信息</param>
		public NetworkWebApiBase(string ipAddress, int port)
		{
			this.ipAddress = ipAddress;
			this.port = port;
#if !NET35 && !NET20
			this.httpClient = new HttpClient();
#endif
		}

		/// <summary>
		/// 使用指定的ip地址，端口号，用户名，密码来初始化对象<br />
		/// Initialize the object with the specified IP address, port number, username, and password
		/// </summary>
		/// <param name="ipAddress">Ip地址信息</param>
		/// <param name="port">端口号信息</param>
		/// <param name="name">用户名</param>
		/// <param name="password">密码</param>
		public NetworkWebApiBase(string ipAddress, int port, string name, string password)
		{
			this.ipAddress = ipAddress;
			this.port = port;
			this.name = name;
			this.password = password;
#if !NET35 && !NET20
			if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(password))
			{
				var handler = new HttpClientHandler { Credentials = new NetworkCredential(name, password) };
				handler.Proxy = null;
				handler.UseProxy = false;

				this.httpClient = new HttpClient(handler);
			}
			else
			{
				this.httpClient = new HttpClient();
			}
#endif
		}

		#endregion

		#region Protect Method

		/// <summary>
		/// 等待重写的额外的指令信息的支持。除了url的形式之外，还支持基于命令的数据交互<br />
		/// Additional instruction information waiting for rewriting is supported.In addition to the url format, command based data interaction is supported
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <returns>是否读取成功的内容</returns>
		protected virtual OperateResult<string> ReadByAddress(string address) => new OperateResult<string>(StringResources.Language.NotSupportedFunction);

#if !NET35 && !NET20
		/// <inheritdoc cref="ReadByAddress(string)"/>
#pragma warning disable CS1998
		protected virtual async Task<OperateResult<string>> ReadByAddressAsync(string address) => new OperateResult<string>(StringResources.Language.NotSupportedFunction);
#pragma warning restore CS1998
#endif
		#endregion

		#region Read Write Support

		/// <summary>
		/// 读取对方信息的的数据信息，通常是针对GET的方法信息设计的。如果使用了url=开头，就表示是使用了原生的地址访问<br />
		/// Read the other side of the data information, usually designed for the GET method information.If you start with url=, you are using native address access
		/// </summary>
		/// <param name="address">无效参数</param>
		/// <returns>带有成功标识的byte[]数组</returns>
		public virtual OperateResult<byte[]> Read(string address)
		{
			OperateResult<string> read = ReadString(address);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			return OperateResult.CreateSuccessResult(Encoding.UTF8.GetBytes(read.Content));
		}

		/// <summary>
		/// 读取对方信息的的字符串数据信息，通常是针对GET的方法信息设计的。如果使用了url=开头，就表示是使用了原生的地址访问<br />
		/// The string data information that reads the other party information, usually designed for the GET method information.If you start with url=, you are using native address access
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <returns>带有成功标识的字符串数据</returns>
		public virtual OperateResult<string> ReadString(string address)
		{
			

			if (address.StartsWith("url=") || address.StartsWith("URL="))
			{
				address = address.Substring(4);
				string url = $"http://{ipAddress}:{port}/{ (address.StartsWith("/") ? address.Substring(1) : address) }";

				try
				{
#if !NET35 && !NET20
					using (HttpResponseMessage response = httpClient.GetAsync(url).Result)
					using (HttpContent content = response.Content)
					{
						response.EnsureSuccessStatusCode();
						string result = content.ReadAsStringAsync().Result;

						return OperateResult.CreateSuccessResult(result);
					}
#else
					WebClient webClient = new WebClient( );
					if (!string.IsNullOrEmpty( name ) || !string.IsNullOrEmpty( password ))
						webClient.Credentials = new NetworkCredential( name, password );

					byte[] content = webClient.DownloadData( url );
					webClient.Dispose( );
					return OperateResult.CreateSuccessResult( Encoding.UTF8.GetString( content ) );
#endif
				}
				catch (Exception ex)
				{
					return new OperateResult<string>(ex.Message);
				}
			}
			else
			{
				return ReadByAddress(address);
			}
		}

		/// <summary>
		/// 使用POST的方式来向对方进行请求数据信息，需要使用url=开头，来表示是使用了原生的地址访问<br />
		/// Using POST to request data information from the other party, we need to start with url= to indicate that we are using native address access
		/// </summary>
		/// <param name="address">指定的地址信息，有些设备可能不支持</param>
		/// <param name="value">原始的字节数据信息</param>
		/// <returns>是否成功的写入</returns>
		public virtual OperateResult Write(string address, byte[] value) => Write(address, Encoding.Default.GetString(value));

		/// <summary>
		/// 使用POST的方式来向对方进行请求数据信息，需要使用url=开头，来表示是使用了原生的地址访问<br />
		/// Using POST to request data information from the other party, we need to start with url= to indicate that we are using native address access
		/// </summary>
		/// <param name="address">指定的地址信息</param>
		/// <param name="value">字符串的数据信息</param>
		/// <returns>是否成功的写入</returns>
		public virtual OperateResult Write(string address, string value)
		{
			if (address.StartsWith("url=") || address.StartsWith("URL="))
			{
				address = address.Substring(4);
				string url = $"http://{ipAddress}:{port}/{ (address.StartsWith("/") ? address.Substring(1) : address) }";

				try
				{
#if !NET35 && !NET20
					using (StringContent stringContent = new StringContent(value))
					using (HttpResponseMessage response = httpClient.PostAsync(url, stringContent).Result)
					using (HttpContent content = response.Content)
					{
						response.EnsureSuccessStatusCode();
						string result = content.ReadAsStringAsync().Result;

						return OperateResult.CreateSuccessResult(result);
					}
#else
					WebClient webClient = new WebClient( );
					webClient.Proxy = null;
					if (!string.IsNullOrEmpty( name ) && !string.IsNullOrEmpty( password ))
						webClient.Credentials = new NetworkCredential( name, password );

					byte[] content = webClient.UploadData( url, Encoding.UTF8.GetBytes( value ) );
					webClient.Dispose( );
					return OperateResult.CreateSuccessResult( Encoding.UTF8.GetString( content ) );
#endif
				}
				catch (Exception ex)
				{
					return new OperateResult<string>(ex.Message);
				}
			}
			else
			{
				return new OperateResult<string>(StringResources.Language.NotSupportedFunction);
			}
		}

		#endregion

		#region Async Read Write Support
#if !NET35 && !NET20
		/// <inheritdoc cref="Read(string)"/>
		public virtual async Task<OperateResult<byte[]>> ReadAsync(string address)
		{
			OperateResult<string> read = await ReadStringAsync(address);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			return OperateResult.CreateSuccessResult(Encoding.UTF8.GetBytes(read.Content));
		}

		/// <inheritdoc cref="ReadString(string)"/>
		public virtual async Task<OperateResult<string>> ReadStringAsync(string address)
		{
			

			if (address.StartsWith("url=") || address.StartsWith("URL="))
			{
				address = address.Substring(4);
				string url = $"http://{ipAddress}:{port}/{ (address.StartsWith("/") ? address.Substring(1) : address) }";

				try
				{
					using (HttpResponseMessage response = await httpClient.GetAsync(url))
					using (HttpContent content = response.Content)
					{
						response.EnsureSuccessStatusCode();
						string result = await content.ReadAsStringAsync();

						return OperateResult.CreateSuccessResult(result);
					}
				}
				catch (Exception ex)
				{
					return new OperateResult<string>(ex.Message);
				}
			}
			else
			{
				return await ReadByAddressAsync(address);
			}
		}

		/// <inheritdoc cref="Write(string, byte[])"/>
		public virtual async Task<OperateResult> WriteAsync(string address, byte[] value) => await WriteAsync(address, Encoding.Default.GetString(value));

		/// <inheritdoc cref="Write(string, string)"/>
		public virtual async Task<OperateResult> WriteAsync(string address, string value)
		{
			if (address.StartsWith("url=") || address.StartsWith("URL="))
			{
				address = address.Substring(4);
				string url = $"http://{ipAddress}:{port}/{ (address.StartsWith("/") ? address.Substring(1) : address) }";

				try
				{
					using (StringContent stringContent = new StringContent(value))
					using (HttpResponseMessage response = await httpClient.PostAsync(url, stringContent))
					using (HttpContent content = response.Content)
					{
						response.EnsureSuccessStatusCode();
						string result = await content.ReadAsStringAsync();

						return OperateResult.CreateSuccessResult(result);
					}
				}
				catch (Exception ex)
				{
					return new OperateResult<string>(ex.Message);
				}
			}
			else
			{
				return new OperateResult<string>(StringResources.Language.NotSupportedFunction);
			}
		}
#endif
		#endregion

		#region Public Properties

		/// <summary>
		/// 获取或设置远程服务器的IP地址<br />
		/// Gets or sets the IP address of the remote server
		/// </summary>
		public string IpAddress
		{
			get => ipAddress;
			set => ipAddress = value;
		}

		/// <summary>
		/// 获取或设置远程服务器的端口号信息<br />
		/// Gets or sets the port number information for the remote server
		/// </summary>
		public int Port
		{
			get => port;
			set => port = value;
		}

		/// <inheritdoc cref="NetworkBase.LogNet"/>
		public ILogNet LogNet { get; set; }

		#endregion

		#region Private Member

		private string ipAddress = "127.0.0.1";
		private int port = 80;
		private string name = string.Empty;
		private string password = string.Empty;

#if !NET35 && !NET20
		private HttpClient httpClient;
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"NetworkWebApiBase[{ipAddress}:{port}]";

		#endregion
	}
}
