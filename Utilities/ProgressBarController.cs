namespace Panoramas_Editor
{
    public class ProgressBarController
    {
        private double _percents;
        public double Percents
        {
            set
            { 
                     if (value <     0.0) { _percents = 0.0  ; }
                else if (value >=  100.0) { _percents = 100.0; }
                else                      { _percents = value; }
            }
            get { return _percents; }
        }

        private double _percentsPerTick;

        public void Initialize(int numOfActions)
        {
            Percents = 0.0;
            if (numOfActions < 1)
            {
                throw new System.Exception($"{nameof(ProgressBarController)}: Количество действий не может быть меньше одного");
            }
            _percentsPerTick = 100.0 / (double)numOfActions;
        }

        public void Tick()
        {
            try { Percents += _percentsPerTick; }
            catch { throw new System.Exception($"{nameof(ProgressBarController)}: Экземпляр класса нужно инициализировать"); }
        }
    }
}
