namespace ElevatorSimulation
{
    class Passenger
    {
        public int StartFloor { get; set; }
        public int DestinationFloor { get; set; }
        // Czas (w sekundach symulacji), kiedy pasażer pojawił się i zaczął czekać
        public int RequestTime { get; set; }
        //Czas oczekiwania na windę oraz podróży na potrzebę statystyki
        public int WaitingTime { get; set; }
        public int TravelTime { get; set; }

        public Passenger(int start, int destination, int requestTime)
        {
            StartFloor = start;
            DestinationFloor = destination;
            RequestTime = requestTime;
        }
    }
}