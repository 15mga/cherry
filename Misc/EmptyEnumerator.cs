namespace Cherry.Misc
{
    public class EmptyEnumerator
    {
        public object Current => null;

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {
        }
    }
}