namespace Chariot.Services
{
    public interface IChatService
    {
        //Create chat (userid, chatname) return room id
        //Join chat (userid, roomcode) return room id 
        //Leave chat (userid, roomid) return displayname
        //Save message (userid, roomid, content) return Message obj
        //Get Messages(var roomid) return messages[]
        //Get Chatrooms(var userid) return chatroom[]
        //Get ChatroomUsers(var roomid) return chatroomuser[]
    }
}
