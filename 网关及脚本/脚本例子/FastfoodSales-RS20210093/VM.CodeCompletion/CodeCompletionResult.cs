using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace VM.CodeCompletion
{
    public class CodeCompletionResult
    {
        public List<ICompletionData> CompletionData = new List<ICompletionData>();
        public ICompletionData SuggestedCompletionDataItem;
        public int TriggerWordLength;
        public string TriggerWord;

        public IOverloadProvider OverloadProvider;
    }
}
