using System;
using System.Collections.Generic;
using System.Text;
using Prism.Mvvm;
using System.Windows;

namespace Mv.Modules.TagManager.Models
{
    public class DeviceItem:BindableBase
    {
        private int _id;
        /// <summary>
        /// 工站ID
        /// </summary>
       public int Id 
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }
        private string _name;
        /// <summary>
        /// 工站名称
        /// </summary>
        public string Name 
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        private int _state;
        /// <summary>
        /// 工站状态
        /// </summary>       
        public int State 
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }
        /// <summary>
        /// 工站产量
        /// </summary>
        private int _yield;
        public int Yield
        {
            get { return _yield; }
            set { SetProperty(ref _yield, value); } 
        }
        private int _prod_N;
        /// <summary>
        /// 工站不良
        /// </summary> 
        public int Prod_N
        {
            get { return _prod_N; }
            set { SetProperty(ref _prod_N, value); }
        }

        private int? _cycle;
        /// <summary>
        /// 生产周期
        /// </summary> 
        public int? Cycle
        {
            get { return _cycle; }
            set { SetProperty(ref _cycle, value); }
        }
        private int _timeStamp;
        public int TimeStamp 
        {
            get { return _timeStamp; }
            set { SetProperty(ref _timeStamp, value); }
        }
        public Visibility Hivevisibility
        {
            get => hivevisibility;
            set => SetProperty(ref hivevisibility, value);
        }
        private Visibility hivevisibility;
        /// <summary>
        /// 状态汇总
        /// </summary> 
        private int? summary;
        public int? Summary
        {
            get { return summary; }
            set { SetProperty(ref summary, value); }
        }
        /// <summary>
        /// 错误编号
        /// </summary> 
        private int? errcode;
        public int? Errcode
        {
            get { return errcode; }
            set { SetProperty(ref errcode, value); }
        }

    }
}
