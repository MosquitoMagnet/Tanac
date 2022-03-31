using DataService;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace Mv.Modules.TagManager.ViewModels
{
    public class TagItem : BindableBase
    {

        public TagItem(ITag tag, string tagName, string tagAddr)
        {
            Tag = tag;
            _tagname = tagName;
            _tagValue = Tag.ToString();
            _addr = tagAddr;
            _timestamp = Tag.TimeStamp;
            Description = Tag.GetMetaData().Description;
            Tag.ValueChanged += new ValueChangedEventHandler(TagValueChanged);
            this.TagValue = Tag.ToString();
        }

        ITag _tag;

        string _tagname;
        public string TagName
        {
            get { return _tagname; }
            set { _tagname = value; }
        }

        string _addr;
        public string Address
        {
            get { return _addr; }
            set { _addr = value; }
        }

        string _tagValue;
        public string TagValue
        {
            get { return _tagValue; }
            set
            {
                SetProperty(ref _tagValue, value);
            }
        }

        DateTime _timestamp;
        public DateTime TimeStamp
        {
            get { return _timestamp; }
            set
            {
                SetProperty(ref _timestamp, value);
            }
        }

        public string Description { get; set; }


        private void TagValueChanged(object sender, ValueChangedEventArgs args)
        {
            TagValue = Tag.ToString();
            TimeStamp = Tag.TimeStamp;
        }

        public int Write(string value)
        {
            if (string.IsNullOrEmpty(value)) return -1;
            if (Tag.Address.VarType == DataType.BOOL)
            {
                if (value == "1") value = "true";
                if (value == "0") value = "false";
            }
            return Tag.Write(value);
        }

        public void SimWrite(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            Storage stor = Storage.Empty;
            try
            {
                if (Tag.Address.VarType == DataType.STR)
                {
                    ((StringTag)Tag).String = value;
                }
                else
                {
                    stor = Tag.ToStorage(value);
                }
                Tag.Update(stor, DateTime.Now, QUALITIES.QUALITY_GOOD);
            }
            catch { }
        }
       public ObservableCollection<TagItem> TagItems { get; set; } = new ObservableCollection<TagItem>();
        public ITag Tag { get => _tag; set => _tag = value; }

        public void Dispose()
        {
            if (Tag != null)
            {
                // ReSharper disable once DelegateSubtraction
               Tag.ValueChanged -= TagValueChanged;
            }
        }
    }
}
