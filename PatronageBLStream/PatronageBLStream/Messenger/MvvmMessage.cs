using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;
using PatronageBLStream.Model;

namespace PatronageBLStream.Messenger
{
    public class MvvmMessage : MessageBase
    {
        public List<PhotoModel> DataList
        {
            get; private set;
        }

        public int BitmapCount
        {
            get; set;
        }

        public MvvmMessage(List<PhotoModel> dataList)
        {
            DataList = dataList;
        }

        public MvvmMessage(int bitmapCount)
        {
            BitmapCount = bitmapCount;
        }
    }
}
