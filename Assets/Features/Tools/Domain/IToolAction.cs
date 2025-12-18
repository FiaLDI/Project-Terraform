namespace Features.Tools.Domain
{
    public interface IToolAction
    {
        void BeginUse();
        void HoldUse();
        void EndUse();
    }
}
