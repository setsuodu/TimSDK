package com.zdkj.plugin;

public class TimMessageType {
    public enum TimMessage {
        Login(0),
        Logout(1),
        GetSelfProfile(2),
        GetUsersProfile(3),
        AddFriend(4),
        DeleteFriends(5),
        DoFriendResponse(6),
        CheckFriends(7),
        GetPendencyList(8),
        GetFriendList(9),
        SendTextElement(10),
        RecvNewMessages(11),
        ModifySelfProfile(12),
        OnForceOffline(13),
        OnUserSigExpired(14),
        SetReadMessage(15),
        GetConversationList(16),
        RevokeMessage(17),
        GetLocalMessage(18),
        GetMessage(19),
        GetLastMsg(20),
        Download(21),
        Image(22),
        Sound(23),
        Video(24),
        ;

        private int type = 0;

        private TimMessage(int type) {
            this.type = type;
        }

        public int value() {
            return this.type;
        }
    }

    public static int TIMFriendAllowType(String type) {
        switch (type) {
            case "AllowType_Type_AllowAny":
                return 1;
            case "AllowType_Type_DenyAny":
                return 2;
            case "AllowType_Type_NeedConfirm":
                return 3;
        }
        return 0;
    }
}
