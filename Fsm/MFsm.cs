using System.Collections.Generic;

namespace Cherry.Fsm
{
    public class MFsm : IMFsm
    {
        private readonly Dictionary<string, IFsm> _nameToFsm = new();

        public MFsm()
        {
            Main = new Cherry.Fsm.Fsm();
            Game.OnQuit += Main.Dispose;
        }

        public IFsm Main { get; }

        public IFsm AddFsm(string name)
        {
            if (_nameToFsm.ContainsKey(name))
            {
                Game.Log.Info($"exist name {name}");
                return null;
            }

            var fsm = new Cherry.Fsm.Fsm();
            _nameToFsm.Add(name, fsm);
            return fsm;
        }

        public void RemoveFsm(string name)
        {
            if (!_nameToFsm.ContainsKey(name))
            {
                Game.Log.Info($"not exist name {name}");
                return;
            }

            _nameToFsm[name].Dispose();
            _nameToFsm.Remove(name);
        }

        public IFsm GetFsm(string name)
        {
            if (_nameToFsm.TryGetValue(name, out var fsm)) return fsm;
            Game.Log.Info($"not exist name {name}");
            return null;
        }
    }
}