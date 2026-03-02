namespace SharpKnP321.Services.Kdf
{
    internal interface IKdfService
    {
        String Dk(String salt, String password);
    }
}
