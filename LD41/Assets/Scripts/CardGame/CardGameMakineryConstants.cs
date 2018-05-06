public static class CardGameMakineryConstants
{
    public static class Queues
    {
        public static readonly string DeckOperation = "DeckOp";
        public static readonly string Hearts = "Hearts";
    }

    public static class Priority
    {
        public static readonly int MapBlock = 10;
        public static readonly int CardDiscard = 50;
        public static readonly int CardUse = 50;
        public static readonly int CardDraw = 100;
        public static readonly int DeckDiscardIntoDraw = 100;
        public static readonly int PawReorganize = 200;
        public static readonly int CardPostDiscard = 250;
        public static readonly int CardShuffle = 250;
        public static readonly int CardAnimations = 500;
        public static readonly int UIAnimations = 1000;
        public static readonly int CharacterAnimations = 700;

    }
}
