namespace Player
{
    public class NormalState : PlayerStateBase
    {
        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            // Dodatkowa logika przy wejœciu do normalnego stanu
        }

        public override void UpdateState(PlayerController player)
        {
            // Wykorzystujemy logikê bazow¹, ale mo¿emy j¹ rozszerzyæ
            base.UpdateState(player);

            // Dodatkowa logika specyficzna dla normalnego stanu
        }

        public override void ExitState(PlayerController player)
        {
            base.ExitState(player);
            // Logika przy wyjœciu z normalnego stanu
        }

        // Mo¿emy nadpisaæ metody bazowe, jeœli chcemy zmieniæ zachowanie poruszania siê lub skakania
    }
}
