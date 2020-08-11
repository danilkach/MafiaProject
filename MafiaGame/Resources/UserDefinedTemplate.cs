using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MafiaClassLibrary;

namespace UserDefined
{
    public class Actions : IAction
    {
        public Actions()
        {
            Container = new List<Object>();
        }
        public void NightAction(City c)
        {

        }
        public void AfterAction(City c)
        {

        }
        public void RegisterPickPlayerHandler(PickPlayerHandler handler)
        {
            PickPlayer += handler;
        }
        public void RegisterServerMessageHandler(ServerMessageHandler handler)
        {
            ServerMessage += handler;
        }
        public void RegisterOnlyPlayerMessageHandler(OnlyPlayerMessage handler)
        {
            PlayerMessage += handler;
        }
        public List<Object> Container { get; set; }
        public event ServerMessageHandler ServerMessage;
        public event PickPlayerHandler PickPlayer;
        public event OnlyPlayerMessage PlayerMessage;
    }
}