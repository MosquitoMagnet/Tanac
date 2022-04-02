using System.Collections;

namespace Mv.Modules.RD402.Service
{
    /// <summary>
    /// lineNumber	生产线编码，提供可配置文件，供用户自定义配置
    ///   station 工位编号，提供配置文件，供用户自定义配置
    ///   machineNO   绕线机设备编码，提供配置文件，供用户自定义配置
    ///   softwareVER 绕线机设备控制软件版本
    ///   moName  工单编码，提供配置文件，供用户自定义配置
    ///   coilWinding 绕线机编码
    ///   axis    绕线机轴号编码
    /// </summary>
    public interface IGetSn
    {
        (bool,string) getsn(Hashtable hashtable);
    }
}