using System.Threading.Tasks;

public interface ICloudSaveService
{
    Task<bool> SavePlayerData(string userId, string json);
    Task<string> LoadPlayerData(string userId);
    Task<bool> SavePlayerStats(string userId, int level, int exp, int[] stats);
}
