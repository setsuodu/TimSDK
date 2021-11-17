using System.IO;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using LitJson;

namespace Client
{
	public class MessageItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		#region Inspector Variables
		[SerializeField] protected VerticalLayoutGroup selfLayout;
		[SerializeField] protected HorizontalLayoutGroup msgBodyContainerLayout;
		[SerializeField] protected VerticalLayoutGroup msgBodyLayout;
		[SerializeField] protected HorizontalLayoutGroup timeContainerLayout;
		[SerializeField] protected Sprite yoursBackground;
		[SerializeField] protected Sprite theirsBackground;

		[SerializeField] protected Mask m_mask;
		[SerializeField] protected LayoutElement m_textLayout;
		[SerializeField] protected LayoutElement m_imageLayout;
		[SerializeField] protected LayoutElement m_soundLayout;

		[SerializeField] protected Text isReadText;
		[SerializeField] protected Text messageText;
		[SerializeField] protected Text timeText;
		[SerializeField] protected Image backgroundImage;
		[SerializeField] protected Image headImage;
		[SerializeField] protected Button m_mainButton; //Button
		#endregion
		#region Member Variables
		[SerializeField] protected MessageInfo messageInfo; //安全级别子元素可以访问
		protected UnityEvent onLongPress = new UnityEvent();
		protected Color yoursColour = new Color(117f / 255f, 84f / 255f, 229f / 255f);
		protected Color theirsColour = new Color(245f / 255f, 245f / 255f, 245f / 255f);
		#endregion

		void Awake()
		{
			onLongPress.AddListener(OnShow);
		}

		void OnDestroy()
		{
			onLongPress.RemoveListener(OnShow);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			Debug.Log($" name={messageInfo.name}, text={messageInfo.text}, isPeerReaded={messageInfo.isPeerReaded}");

			MenuItemClass[] items = new MenuItemClass[]
			{
				new MenuItemClass("删除消息", Action1),
				new MenuItemClass("撤回消息", Action2),
			};
			LongPressManager.Init(eventData.position, items);
			Invoke("OnLongPress", 1f); //长按1s
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			CancelInvoke("OnLongPress");
		}

		void OnLongPress()
		{
			onLongPress.Invoke();
		}

		public void OnShow()
		{
			LongPressManager.Show();
		}

		void Action1()
		{
			Debug.Log("删除消息");
			bool result = TimSdkManager.Instance.RemoveMessage("");
		}

		void Action2()
		{
			Debug.Log("撤回消息");
		}

		#region Public Methods

		public virtual void Setup(MessageInfo info, bool hideTime)
		{
			this.messageInfo = info;

			isReadText.text = messageInfo.isPeerReaded ? "已读" : "未读"; //对方是否已读我的消息，标记在我的发言上
			messageText.text = messageInfo.text;
			timeText.text = messageInfo.timeStr;
			timeText.gameObject.SetActive(!hideTime);

			switch (messageInfo.messageType)
			{
				case MessageType.Yours:
					{
						backgroundImage.color = yoursColour;
						messageText.color = Color.white;

						backgroundImage.sprite = yoursBackground;

						selfLayout.padding.left = 25;
						selfLayout.padding.right = 10;

						msgBodyContainerLayout.childAlignment = TextAnchor.UpperRight;
						msgBodyContainerLayout.padding.left = 5; //聊天气泡到最左边间距
						msgBodyContainerLayout.padding.right = 0; //聊天气泡到最右边间距

						msgBodyLayout.padding.left = 10;
						msgBodyLayout.padding.right = 20;

						timeContainerLayout.padding.left = 0;
						timeContainerLayout.padding.right = 50; //50
						timeText.alignment = TextAnchor.MiddleCenter; //MiddleRight

						//设置兄弟姐妹顺序
						isReadText.transform.SetSiblingIndex(0);
						messageText.transform.SetSiblingIndex(1);
						headImage.transform.SetSiblingIndex(2);
						isReadText.gameObject.SetActive(true);
					}
					break;
				case MessageType.Theirs:
					{
						backgroundImage.color = theirsColour;
						messageText.color = Color.black;

						backgroundImage.sprite = theirsBackground;

						selfLayout.padding.left = 10;
						selfLayout.padding.right = 25;

						msgBodyContainerLayout.childAlignment = TextAnchor.UpperLeft;
						msgBodyContainerLayout.padding.left = 0; //聊天气泡到最左边间距
						msgBodyContainerLayout.padding.right = 5; //聊天气泡到最右边间距

						msgBodyLayout.padding.left = 20;
						msgBodyLayout.padding.right = 10;

						timeContainerLayout.padding.left = 50; //50
						timeContainerLayout.padding.right = 0;
						timeText.alignment = TextAnchor.MiddleCenter; //MiddleLeft

						//设置子物体顺序
						headImage.transform.SetSiblingIndex(0);
						messageText.transform.SetSiblingIndex(1);
						isReadText.transform.SetSiblingIndex(2);
						isReadText.gameObject.SetActive(false);
					}
					break;
			}

			switch (messageInfo.elemType)
			{
				case TIMElemType.Text:
					{
						//文本元素->打开////////////////////////
						m_textLayout.ignoreLayout = false;
						m_textLayout.gameObject.SetActive(true);
						//图片元素->关闭
						m_mask.enabled = false;
						m_imageLayout.ignoreLayout = true;
						m_imageLayout.gameObject.SetActive(false);
						//音频元素->关闭
						m_soundLayout.ignoreLayout = true;
						m_soundLayout.gameObject.SetActive(false);
					}
					break;
				case TIMElemType.Image:
					{
						//文本元素->关闭
						m_textLayout.ignoreLayout = true;
						m_textLayout.gameObject.SetActive(false);
						//图片元素->打开////////////////////////
						m_mask.enabled = true;
						m_imageLayout.ignoreLayout = false;
						m_imageLayout.gameObject.SetActive(true);
						//--->图片衍生事件
						//Debug.Log($"图片衍生事件\ntext={info.text}\njson={info.param}");
						if (string.IsNullOrEmpty(info.param))
						{
							//我发的图片
							FileManager.Load(info.text, OnLoadMessageImage); //本地图片存在
						}
						else
						{
							var obj = JsonMapper.ToObject<TIMImageResp>(info.param);
							if (File.Exists(info.text))
							{
								//这里的text是sdk中用uuid拼接返回的
								//"/storage/emulated/0/Android/data/com.setsuodu.timsdk/files//0_1400286941_2_b31982f7fa967ae67fcede0be570245b.png",
								FileManager.Load(info.text, OnLoadMessageImage); //本地图片存在
							}
							else
							{
								//手动拼接uuid
								FileManager.Download(obj.url, OnLoadMessageImage, obj.uuid); //下载
							}
						}
						//音频元素->关闭
						m_soundLayout.ignoreLayout = true;
						m_soundLayout.gameObject.SetActive(false);
					}
					break;
				case TIMElemType.Sound:
					{
						//文本元素->关闭
						m_textLayout.ignoreLayout = true;
						m_textLayout.gameObject.SetActive(false);
						//图片元素->关闭
						m_mask.enabled = true;
						m_imageLayout.ignoreLayout = true;
						m_imageLayout.gameObject.SetActive(false);
						//音频元素->打开////////////////////////
						m_soundLayout.ignoreLayout = false;
						m_soundLayout.gameObject.SetActive(true);
					}
					break;
			}
		}

		void OnLoadMessageImage(byte[] bytes)
		{
			Texture2D t2d = new Texture2D(2, 2);
			t2d.LoadImage(bytes);
			Sprite sp = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
			m_mainButton.image.sprite = sp;
			m_mainButton.image.type = Image.Type.Simple;
			m_mainButton.image.preserveAspect = true;

			float aspect = (float)t2d.width / (float)t2d.height;
			if (aspect > 1) //横向
			{
				m_imageLayout.minWidth = 128f * aspect;
				m_imageLayout.minHeight = 128f;
			}
			else //竖向
			{
				m_imageLayout.minWidth = 128f;
				m_imageLayout.minHeight = 128f / aspect;
			}
		}

		// 解析emoji
		string DecodeUTF()
		{
			string text = messageText.text;
			bool utf = text.Contains(@"\U000");
			if (utf && text.Contains("-"))
			{
				text = text.Replace("-", @"\U000");
			}
			string str = utf ? @"\\U(?<Value>[a-zA-Z0-9]{8})" : @"\\U(?<Value>[a-zA-Z0-9]{32})"; //{20}
			return Regex.Replace(text, str, m => char.ConvertFromUtf32(int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)));
		}

		#endregion
	}
}
