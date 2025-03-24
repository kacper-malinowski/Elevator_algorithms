namespace ElevatorSimulation
{
    class Passenger
    {
        public int StartFloor { get; set; }
        public int DestinationFloor { get; set; }
        // Czas (w sekundach symulacji), kiedy pasażer pojawił się i zaczął czekać
        public int RequestTime { get; set; }

        public Passenger(int start, int destination, int requestTime)
        {
            StartFloor = start;
            DestinationFloor = destination;
            RequestTime = requestTime;
        }
    }
}