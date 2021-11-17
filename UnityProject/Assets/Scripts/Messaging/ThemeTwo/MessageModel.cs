namespace Client
{
	public enum MessageType
	{
		Theirs,
		Yours,
		Count
	}

	[System.Serializable]
	public class MessageInfo
	{
		public string name; //用户昵称
		public string timeStr;
		public MessageType messageType; //isSelf
		public bool isRead;
		public bool isPeerReaded;
		public TIMElemType elemType; //文字/图片/音频/视频..
		public string text;
		public string param;
	}

	[System.Serializable]
	public class TIMChatMsg
	{
		public long seq;
		public long rand;
		public long timestamp;
		public bool isSelf;
		public bool isRead;
		public bool isPeerReaded;
		public int elemType;
		public int subType;
		public string text; //Text/Path
		public string param; //备注(Json)
	}

	[System.Serializable]
	public struct PosStringTuple
	{
		public int pos;
		public string emoji;
		public PosStringTuple(int p, string s)
		{
			this.pos = p;
			this.emoji = s;
		}
	}
}
