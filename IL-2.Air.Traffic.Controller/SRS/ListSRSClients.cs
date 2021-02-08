namespace IL_2.Air.Traffic.Controller.SRS
{
    public class ListSRSClients
    {
        public string ClientGuid { get; set; }
        public string Name { get; set; }
        public int Seat { get; set; }
        public int Coalition { get; set; }
        public PlayerGameState GameState { get; set; }
    }
}
