namespace Player
{
    public class NormalState : PlayerStateBase
    {
        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            // Dodatkowa logika przy wej�ciu do normalnego stanu
        }

        public override void UpdateState(PlayerController player)
        {
            // Wykorzystujemy logik� bazow�, ale mo�emy j� rozszerzy�
            base.UpdateState(player);

            // Dodatkowa logika specyficzna dla normalnego stanu
        }

        public override void ExitState(PlayerController player)
        {
            base.ExitState(player);
            // Logika przy wyj�ciu z normalnego stanu
        }

        // Mo�emy nadpisa� metody bazowe, je�li chcemy zmieni� zachowanie poruszania si� lub skakania
    }
}
