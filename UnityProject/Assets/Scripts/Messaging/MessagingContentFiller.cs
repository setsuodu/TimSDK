using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace Client
{
	public class MessagingContentFiller : MonoBehaviour, IContentFiller
	{
		#region Inspector Variables

		[SerializeField] private List<MessageInfo> messageInfos;
		[SerializeField] private GameObject themeTwoItemPrefab;
		public ListScrollRect listScrollRect;

		#endregion

		#region Unity Methods

		void Awake()
		{
			messageInfos = new List<MessageInfo>();
		}

		#endregion

		#region IContentFiller Methods

		public GameObject GetListItem(int index, int itemType, GameObject obj)
		{
			if (obj == null)
			{
				obj = Instantiate(themeTwoItemPrefab);
			}

			MessageInfo messageInfo = messageInfos[index];
			bool hideTime = !IsLastMessageInChain(index);

			MessageItem messageItem = obj.GetComponent<MessageItem>();
			messageItem.Setup(messageInfo, hideTime);

			return obj;
		}

		public int GetItemCount()
		{
			return messageInfos.Count;
		}

		public int GetItemType(int index)
		{
			// There will only ever be one item type being used at a time (ThemeOne or ThemeTwo)
			return 0;
		}

		public void OnSendButtonClicked(TIMChatMsg msg)
		{
			if (!string.IsNullOrEmpty(msg.text))
			{
				MessageInfo messageInfo = new MessageInfo();
				messageInfo.name = "You";
				messageInfo.timeStr = Utils.ConvertTimestamp(msg.timestamp).ToString();
				messageInfo.messageType = msg.isSelf ? MessageType.Yours : MessageType.Theirs;
				messageInfo.isRead = msg.isRead;
				messageInfo.isPeerReaded = msg.isPeerReaded;
				messageInfo.elemType = (TIMElemType)msg.elemType;
				messageInfo.text = msg.text;
				messageInfo.param = msg.param;
				messageInfos.Add(messageInfo);

				listScrollRect.RefreshContent(); //build
				if (messageInfos.Count > 0)
					listScrollRect.GoToListItem(messageInfos.Count - 1); //goto
			}
		}

		public void LoadChatData(List<TIMChatMsg> list, int gotoIndex)
		{
			if (list.Count <= 0) return;

			for (int i = 0; i < list.Count; i++)
			{
				MessageInfo messageInfo = new MessageInfo();
				messageInfo.name = "test";
				messageInfo.timeStr = Utils.ConvertTimestamp(list[i].timestamp).ToString();
				messageInfo.messageType = list[i].isSelf ? MessageType.Yours : MessageType.Theirs;
				messageInfo.isRead = list[i].isRead;
				messageInfo.isPeerReaded = list[i].isPeerReaded;
				messageInfo.elemType = (TIMElemType)list[i].elemType;
				messageInfo.text = list[i].text;
				messageInfo.param = list[i].param;
				//TODO: 去重
				messageInfos.Insert(0, messageInfo);
			}

			listScrollRect.RefreshContent();
			if (messageInfos.Count > 0)
			{
				//有刷新内容
				//Debug.Log($"goto={gotoIndex}");
				listScrollRect.GoToListItem(gotoIndex);
				listScrollRect.StopMovement();
			}
		}

		public void SetRead()
		{
			for (int i = 0; i < messageInfos.Count; i++) 
			{
				var info = messageInfos[i];
				if (info.messageType == MessageType.Yours)
				{
					info.isPeerReaded = true; //对方已读
				}
			}
			listScrollRect.RefreshContent();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Checks if MessageInfo at index is the first message in a chain of messages from the same person (ei. MessageType)
		/// </summary>
		private bool IsFirstMessageInChain(int index)
		{
			if (index == 0)
			{
				return true;
			}

			MessageType messageType1 = messageInfos[index].messageType;
			MessageType messageType2 = messageInfos[index - 1].messageType;

			return messageType1 != messageType2;
		}

		/// <summary>
		/// Checks if MessageInfo at index is the last message in a chain of messages from the same person (ei. MessageType)
		/// </summary>
		private bool IsLastMessageInChain(int index)
		{
			if (index == messageInfos.Count - 1)
			{
				return true;
			}

			MessageType messageType1 = messageInfos[index].messageType;
			MessageType messageType2 = messageInfos[index + 1].messageType;

			return messageType1 != messageType2;
		}

		private static readonly string[] randomPuns = new string[]
		{
		"Did you hear about the guy whose whole left side was cut off? He's all right now.",
		"I wondered why the baseball was getting bigger. Then it hit me.",
		"I'm reading a book about anti-gravity. It's impossible to put down.",
		"I'd tell you a chemistry joke but I know I wouldn't get a reaction.",
		"I used to be a banker but I lost interest.",
		"It's not that the man did not know how to juggle, he just didn't have the balls to do it.",
		"Did you hear about the guy who got hit in the head with a can of soda? He was lucky it was a soft drink.",
		"So what if I don't know what apocalypse means!? It's not the end of the world!",
		"A friend of mine tried to annoy me with bird puns, but I soon realized that toucan play at that game.",
		"Have you ever tried to eat a clock? It's very time consuming.",
		"Claustrophobic people are more productive thinking outside the box.",
		"Yesterday I accidentally swallowed some food coloring. The doctor says I'm OK, but I feel like I've dyed a little inside.",
		"Need an ark to save two of every animal? I noah guy.",
		"I couldn't quite remember how to throw a boomerang, but eventually it came back to me.",
		"My friend's bakery burned down last night. Now his business is toast.",
		};

		/// <summary>
		/// Creates a bunch of random messages to be displayed in the list
		/// </summary>
		private void CreateDefaultStartingMessages()
		{
			System.DateTime time = System.DateTime.Now;

			for (int i = 0; i < 50; i++)
			{
				MessageInfo messageInfo = new MessageInfo();

				messageInfo.messageType = (MessageType)Random.Range(0, (int)MessageType.Count);

				switch (messageInfo.messageType)
				{
					case MessageType.Yours:
						messageInfo.name = "You";
						break;
					case MessageType.Theirs:
						messageInfo.name = "John Doe";
						break;
				}

				messageInfo.text = randomPuns[Random.Range(0, randomPuns.Length)];
				messageInfo.timeStr = ParseDateTime(time);

				time = time.Add(new System.TimeSpan(0, -Random.Range(1, 11), 0));

				messageInfos.Insert(0, messageInfo);
			}
		}

		/// <summary>
		/// Parses the date time and returns a string to be displayed
		/// </summary>
		private string ParseDateTime(System.DateTime time)
		{
			string[] months = { "Jan", "Feb", "Mar", "April", "May", "June", "July", "Aug", "Sept", "Oct", "Nov", "Dec" };

			string ampm = (time.Hour >= 12) ? "pm" : "am";
			int hour = time.Hour;

			if (hour == 0)
			{
				hour = 12;
			}
			else if (hour > 12)
			{
				hour -= 12;
			}

			string minuteStr = time.Minute < 10 ? "0" + time.Minute : time.Minute.ToString();

			return string.Format("{0}, {1} {2} {3}:{4}{5}", months[time.Month - 1], time.Day, time.Year, hour, minuteStr, ampm);
		}

        #endregion

        #region 测试代码
        public void LoadMoreData(int pageCount)
		{
			string[] urls = new string[]
			{
				"http://yesky1.zz314.njxzwh.com/2017-05-26/519bd77a13bb3a5cfd11f6ec2fe5f486.png",
				"http://yesky1.zz314.njxzwh.com/2017-05-26/4d4af8970dee793c215c9d7d8619dc05.jpg",
			};
			string[] uuids = new string[]
			{
				"a1-b2-c3-d4.png",
				"1a-2b-3c-4d.png",
			};

			List<TIMChatMsg> tmp = new List<TIMChatMsg>();
			for (int i = 0; i < 5; i++)
			{
				int seed = UnityEngine.Random.Range(0, urls.Length);
				TIMImageResp resp = new TIMImageResp();
				resp.type = 0; //Original(0), Thumb(1), Large(2);
				resp.url = urls[seed];
				resp.uuid = uuids[seed]; //"0_1400286941_2_b31982f7fa967ae67fcede0be570245b.png",
				resp.size = 1024;
				resp.height = 128;
				resp.width = 128;
				string json = JsonMapper.ToJson(resp);

				bool self = i % 2 == 0;
				bool type = i % 3 == 0;
				var info = new TIMChatMsg();
				info.seq = 0;
				info.rand = 0;
				info.timestamp = Utils.ToTimestamp(System.DateTime.Now);
				info.isSelf = self;
				info.isRead = false;
				info.isPeerReaded = false;
				info.elemType = type ? (int)TIMElemType.Text : (int)TIMElemType.Image;
				info.text = type ? $"abcd-{messageInfos.Count}" : System.IO.Path.Combine(Application.persistentDataPath, "/FileCache/Image/", resp.uuid);
				//@"C:/Users/Administrator/AppData/LocalLow/setsuodu/TimSdk\Splash.png";
				//"/storage/emulated/0/Android/data/com.setsuodu.timsdk/files//0_1400286941_2_b31982f7fa967ae67fcede0be570245b.png",
				info.param = type ? $"" : json;
				tmp.Insert(0, info);
			}
			LoadChatData(tmp, pageCount);
		}
		#endregion
	}
}