using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Apis.Extensions;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Api;
using Unity.Services.CloudSave.Model;

namespace UsernameRegistry;

public class UsernameRegistryModule
{
    private const string RegistryKey = "names";

    private const int ShardCount = 16;

    private const int MaximumAttempts = 5;


    // ============================================================
    // CLAIM USERNAME
    // ============================================================

    [CloudCodeFunction("ClaimUsername")]
    public async Task<UsernameResult> ClaimUsername(
        IExecutionContext context,
        IGameApiClient gameApiClient,
        string requestedName)
    {
        string displayName;

        try
        {
            displayName = ValidateAndCleanName(requestedName);
        }
        catch (Exception exception)
        {
            return new UsernameResult
            {
                Success = false,
                Username = null,
                Error = exception.Message
            };
        }

        string normalizedName =
            NormalizeName(displayName);

        string shardId =
            GetShardId(normalizedName);

        var cloudSave =
            gameApiClient.CloudSaveData;


        for (int attempt = 0;
             attempt < MaximumAttempts;
             attempt++)
        {
            RegistryShard result =
                await GetRegistryShard(
                    context,
                    cloudSave,
                    shardId
                );

            Dictionary<string, UsernameEntry> names =
                result.Names;


            // ----------------------------------------------------
            // Check whether the username already exists.
            // ----------------------------------------------------

            if (names.TryGetValue(
                normalizedName,
                out UsernameEntry existingEntry))
            {
                // The player already owns this username.
                if (existingEntry.PlayerId ==
                    context.PlayerId)
                {
                    return new UsernameResult
                    {
                        Success = true,
                        Username = existingEntry.DisplayName,
                        Error = null
                    };
                }


                // Somebody else owns it.
                return new UsernameResult
                {
                    Success = false,
                    Username = null,
                    Error = "UsernameAlreadyTaken"
                };
            }


            // ----------------------------------------------------
            // Add the username to the Registry.
            // ----------------------------------------------------

            names[normalizedName] =
                new UsernameEntry
                {
                    PlayerId = context.PlayerId,
                    DisplayName = displayName
                };


            string serializedNames =
                JsonSerializer.Serialize(names);


            try
            {
                await cloudSave.SetPrivateCustomItemAsync(
                    context,
                    context.ServiceToken,
                    context.ProjectId!,
                    shardId,
                    new SetItemBody(
                        RegistryKey,
                        serializedNames,
                        result.WriteLock
                    )
                );


                return new UsernameResult
                {
                    Success = true,
                    Username = displayName,
                    Error = null
                };
            }
            catch (ApiException exception)
            {
                // Another request modified this shard between
                // reading it and writing it.
                //
                // Try again using the new WriteLock.

                if (exception.Response.StatusCode ==
                    HttpStatusCode.Conflict)
                {
                    continue;
                }

                throw;
            }
        }


        return new UsernameResult
        {
            Success = false,
            Username = null,
            Error = "RegistryBusy"
        };
    }


    // ============================================================
    // CHECK USERNAME AVAILABILITY
    // ============================================================

    [CloudCodeFunction("IsUsernameAvailable")]
    public async Task<bool> IsUsernameAvailable(
        IExecutionContext context,
        IGameApiClient gameApiClient,
        string requestedName)
    {
        string displayName =
            ValidateAndCleanName(requestedName);

        string normalizedName =
            NormalizeName(displayName);

        string shardId =
            GetShardId(normalizedName);


        RegistryShard result =
            await GetRegistryShard(
                context,
                gameApiClient.CloudSaveData,
                shardId
            );


        return !result.Names.ContainsKey(
            normalizedName
        );
    }


    // ============================================================
    // RELEASE CURRENT PLAYER'S USERNAME
    // ============================================================

    [CloudCodeFunction("ReleaseUsername")]
    public async Task<bool> ReleaseUsername(
        IExecutionContext context,
        IGameApiClient gameApiClient)
    {
        var cloudSave =
            gameApiClient.CloudSaveData;


        // We have to check every shard because we do not
        // necessarily know which username the player owns.

        for (int shard = 0;
             shard < ShardCount;
             shard++)
        {
            string shardId =
                GetShardIdFromNumber(shard);


            for (int attempt = 0;
                 attempt < MaximumAttempts;
                 attempt++)
            {
                RegistryShard result =
                    await GetRegistryShard(
                        context,
                        cloudSave,
                        shardId
                    );


                string keyToRemove = null;


                // ------------------------------------------------
                // Find the username belonging to this Player ID.
                // ------------------------------------------------

                foreach (var entry in result.Names)
                {
                    if (entry.Value.PlayerId ==
                        context.PlayerId)
                    {
                        keyToRemove =
                            entry.Key;

                        break;
                    }
                }


                // This shard does not contain the player's
                // username.
                if (keyToRemove == null)
                    break;


                result.Names.Remove(
                    keyToRemove
                );


                string serializedNames =
                    JsonSerializer.Serialize(
                        result.Names
                    );


                try
                {
                    await cloudSave.SetPrivateCustomItemAsync(
                        context,
                        context.ServiceToken,
                        context.ProjectId!,
                        shardId,
                        new SetItemBody(
                            RegistryKey,
                            serializedNames,
                            result.WriteLock
                        )
                    );


                    return true;
                }
                catch (ApiException exception)
                {
                    if (exception.Response.StatusCode ==
                        HttpStatusCode.Conflict)
                    {
                        // Re-read the shard and try again.
                        continue;
                    }

                    throw;
                }
            }
        }


        return false;
    }


    // ============================================================
    // REMOVE SPECIFIC USERNAME
    //
    // DEVELOPMENT / TESTING ONLY
    // ============================================================

    [CloudCodeFunction("RemoveUsername")]
    public async Task<bool> RemoveUsername(
        IExecutionContext context,
        IGameApiClient gameApiClient,
        string requestedName)
    {
        string displayName =
            ValidateAndCleanName(requestedName);

        string normalizedName =
            NormalizeName(displayName);

        string shardId =
            GetShardId(normalizedName);


        var cloudSave =
            gameApiClient.CloudSaveData;


        for (int attempt = 0;
             attempt < MaximumAttempts;
             attempt++)
        {
            RegistryShard result =
                await GetRegistryShard(
                    context,
                    cloudSave,
                    shardId
                );


            // ----------------------------------------------------
            // Check whether the username exists.
            // ----------------------------------------------------

            if (!result.Names.ContainsKey(
                normalizedName))
            {
                return false;
            }


            // ----------------------------------------------------
            // Remove it.
            // ----------------------------------------------------

            result.Names.Remove(
                normalizedName
            );


            string serializedNames =
                JsonSerializer.Serialize(
                    result.Names
                );


            try
            {
                await cloudSave.SetPrivateCustomItemAsync(
                    context,
                    context.ServiceToken,
                    context.ProjectId!,
                    shardId,
                    new SetItemBody(
                        RegistryKey,
                        serializedNames,
                        result.WriteLock
                    )
                );


                return true;
            }
            catch (ApiException exception)
            {
                if (exception.Response.StatusCode ==
                    HttpStatusCode.Conflict)
                {
                    continue;
                }

                throw;
            }
        }


        return false;
    }


    // ============================================================
    // GET USERNAME
    // ============================================================

    [CloudCodeFunction("GetUsername")]
    public async Task<string> GetUsername(
        IExecutionContext context,
        IGameApiClient gameApiClient,
        string playerId)
    {
        for (int shard = 0;
             shard < ShardCount;
             shard++)
        {
            string shardId =
                GetShardIdFromNumber(shard);


            RegistryShard result =
                await GetRegistryShard(
                    context,
                    gameApiClient.CloudSaveData,
                    shardId
                );


            foreach (var entry in result.Names)
            {
                if (entry.Value.PlayerId ==
                    playerId)
                {
                    return entry.Value.DisplayName;
                }
            }
        }


        return null;
    }


    // ============================================================
    // INITIALIZE REGISTRY
    // ============================================================

    [CloudCodeFunction("InitializeRegistry")]
    public async Task InitializeRegistry(
        IExecutionContext context,
        IGameApiClient gameApiClient)
    {
        var cloudSave =
            gameApiClient.CloudSaveData;


        for (int shard = 0;
             shard < ShardCount;
             shard++)
        {
            string shardId =
                GetShardIdFromNumber(shard);


            var existing =
                await cloudSave.GetPrivateCustomItemsAsync(
                    context,
                    context.ServiceToken,
                    context.ProjectId!,
                    shardId,
                    new List<string>
                    {
                        RegistryKey
                    }
                );


            // Shard already exists.
            if (existing.Data.Results.Any())
                continue;


            await cloudSave.SetPrivateCustomItemAsync(
                context,
                context.ServiceToken,
                context.ProjectId!,
                shardId,
                new SetItemBody(
                    RegistryKey,
                    "{}"
                )
            );
        }
    }


    // ============================================================
    // GET REGISTRY SHARD
    // ============================================================

    private async Task<RegistryShard> GetRegistryShard(
        IExecutionContext context,
        ICloudSaveDataApi cloudSave,
        string shardId)
    {
        var result =
            await cloudSave.GetPrivateCustomItemsAsync(
                context,
                context.ServiceToken,
                context.ProjectId!,
                shardId,
                new List<string>
                {
                    RegistryKey
                }
            );


        var item =
            result.Data.Results.FirstOrDefault();


        // Shard does not exist yet.
        if (item == null)
        {
            return new RegistryShard
            {
                Names =
                    new Dictionary<string, UsernameEntry>(),

                WriteLock = null
            };
        }


        Dictionary<string, UsernameEntry> names;


        try
        {
            names =
                JsonSerializer.Deserialize<
                    Dictionary<string, UsernameEntry>
                >(
                    item.Value?.ToString() ?? "{}"
                )
                ??
                new Dictionary<
                    string,
                    UsernameEntry
                >();
        }
        catch
        {
            names =
                new Dictionary<
                    string,
                    UsernameEntry
                >();
        }


        return new RegistryShard
        {
            Names = names,
            WriteLock = item.WriteLock
        };
    }


    // ============================================================
    // VALIDATE USERNAME
    // ============================================================

    private static string ValidateAndCleanName(
        string requestedName)
    {
        if (string.IsNullOrWhiteSpace(
            requestedName))
        {
            throw new Exception(
                "Username cannot be empty."
            );
        }


        string name =
            requestedName.Trim();


        if (name.Length < 1)
        {
            throw new Exception(
                "Username is too short."
            );
        }


        if (name.Length > 20)
        {
            throw new Exception(
                "Username is too long."
            );
        }


        if (name.Contains(" "))
        {
            throw new Exception(
                "Username cannot contain spaces."
            );
        }


        foreach (char character in name)
        {
            if (!char.IsLetterOrDigit(
                    character)
                &&
                character != '_'
                &&
                character != '-')
            {
                throw new Exception(
                    "Username contains an invalid character."
                );
            }
        }


        return name;
    }


    // ============================================================
    // NORMALIZE USERNAME
    // ============================================================

    private static string NormalizeName(
        string name)
    {
        return name.Trim().ToLowerInvariant();
    }


    // ============================================================
    // GET SHARD ID
    // ============================================================

    private static string GetShardId(
        string normalizedName)
    {
        using SHA256 sha256 =
            SHA256.Create();


        byte[] hash =
            sha256.ComputeHash(
                Encoding.UTF8.GetBytes(
                    normalizedName
                )
            );


        int shard =
            hash[0] % ShardCount;


        return GetShardIdFromNumber(
            shard
        );
    }


    // ============================================================
    // GET SHARD ID FROM NUMBER
    // ============================================================

    private static string GetShardIdFromNumber(
        int shard)
    {
        return
            $"username-registry-{shard:D2}";
    }


    // ============================================================
    // REGISTRY SHARD
    // ============================================================

    private class RegistryShard
    {
        public Dictionary<string, UsernameEntry> Names
        {
            get;
            set;
        }

        public string WriteLock
        {
            get;
            set;
        }
    }


    // ============================================================
    // USERNAME ENTRY
    // ============================================================

    private class UsernameEntry
    {
        public string PlayerId
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }
    }


    // ============================================================
    // USERNAME RESULT
    // ============================================================

    public class UsernameResult
    {
        public bool Success
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }

        public string Error
        {
            get;
            set;
        }
    }
}



/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Apis.Extensions;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;
using Unity.Services.CloudSave.Api;

namespace UsernameRegistry;

public class UsernameRegistryModule
{
    private const string RegistryKey = "names";

    // 16 registry shards.
    //
    // Each shard is a separate Cloud Save Custom Item.
    // This prevents us from putting every username into one
    // Cloud Save entity.
    private const int ShardCount = 16;

    // ---------------------------------------------------------
    // CLAIM USERNAME
    // ---------------------------------------------------------

    [CloudCodeFunction("ClaimUsername")]
    public async Task<UsernameResult> ClaimUsername(
        IExecutionContext context,
        IGameApiClient gameApiClient,
        string requestedName)
    {
        string displayName = ValidateAndCleanName(requestedName);

        string normalizedName = NormalizeName(displayName);

        string shardId = GetShardId(normalizedName);

        var cloudSave = gameApiClient.CloudSaveData;

        for (int attempt = 0; attempt < 5; attempt++)
        {
            var result = await GetRegistryShard(
                context,
                cloudSave,
                shardId);

            Dictionary<string, string> names =
                result.Names;

            // Does somebody already own this name?
            if (names.TryGetValue(normalizedName, out string existingPlayerId))
            {
                // If this player already owns the name,
                // consider the operation successful.
                if (existingPlayerId == context.PlayerId)
                {
                    return new UsernameResult
                    {
                        Success = true,
                        Username = displayName,
                        Error = null
                    };
                }

                return new UsernameResult
                {
                    Success = false,
                    Username = null,
                    Error = "UsernameAlreadyTaken"
                };
            }

            // Claim it.
            names[normalizedName] = context.PlayerId;

            string serializedNames =
                JsonSerializer.Serialize(names);

            try
            {
                await cloudSave.SetPrivateCustomItemAsync(
                    context,
                    context.ServiceToken,
                    context.ProjectId!,
                    shardId,
                    new SetItemBody(
                        RegistryKey,
                        serializedNames,
                        result.WriteLock
                    )
                );

                return new UsernameResult
                {
                    Success = true,
                    Username = displayName,
                    Error = null
                };
            }
            catch (ApiException exception)
            {
                // Another player changed this shard while we
                // were working on it.
                //
                // Retry by reading the newest version.
                if (exception.Response.StatusCode ==
                    HttpStatusCode.Conflict)
                {
                    continue;
                }

                throw;
            }
        }

        return new UsernameResult
        {
            Success = false,
            Username = null,
            Error = "RegistryBusy"
        };
    }

    // ---------------------------------------------------------
    // CHECK USERNAME
    // ---------------------------------------------------------

    [CloudCodeFunction("IsUsernameAvailable")]
    public async Task<bool> IsUsernameAvailable(
        IExecutionContext context,
        IGameApiClient gameApiClient,
        string requestedName)
    {
        string displayName = ValidateAndCleanName(requestedName);

        string normalizedName = NormalizeName(displayName);

        string shardId = GetShardId(normalizedName);

        var result = await GetRegistryShard(
            context,
            gameApiClient.CloudSaveData,
            shardId);

        return !result.Names.ContainsKey(normalizedName);
    }

    // ---------------------------------------------------------
    // RELEASE USERNAME
    // ---------------------------------------------------------

    [CloudCodeFunction("ReleaseUsername")]
    public async Task<bool> ReleaseUsername(
        IExecutionContext context,
        IGameApiClient gameApiClient)
    {
        // Find the player's username.
        //
        // We don't yet know which shard contains it, so we
        // search the shards.
        for (int shard = 0; shard < ShardCount; shard++)
        {
            string shardId =
                GetShardIdFromNumber(shard);

            var result = await GetRegistryShard(
                context,
                gameApiClient.CloudSaveData,
                shardId);

            string keyToRemove = null;

            foreach (var entry in result.Names)
            {
                if (entry.Value == context.PlayerId)
                {
                    keyToRemove = entry.Key;
                    break;
                }
            }

            if (keyToRemove == null)
                continue;

            result.Names.Remove(keyToRemove);

            string serializedNames =
                JsonSerializer.Serialize(result.Names);

            try
            {
                await gameApiClient.CloudSaveData.SetPrivateCustomItemAsync(
                    context,
                    context.ServiceToken,
                    context.ProjectId!,
                    shardId,
                    new SetItemBody(
                        RegistryKey,
                        serializedNames,
                        result.WriteLock
                    )
                );

                return true;
            }
            catch (ApiException exception)
            {
                if (exception.Response.StatusCode ==
                    HttpStatusCode.Conflict)
                {
                    // Try again from the beginning.
                    return await ReleaseUsername(
                        context,
                        gameApiClient);
                }

                throw;
            }
        }

        return false;
    }

    // ---------------------------------------------------------
    // GET USERNAME FOR PLAYER
    // ---------------------------------------------------------

    [CloudCodeFunction("GetUsername")]
    public async Task<string> GetUsername(
        IExecutionContext context,
        IGameApiClient gameApiClient,
        string playerId)
    {
        // Search all registry shards.
        for (int shard = 0; shard < ShardCount; shard++)
        {
            string shardId =
                GetShardIdFromNumber(shard);

            var result = await GetRegistryShard(
                context,
                gameApiClient.CloudSaveData,
                shardId);

            foreach (var entry in result.Names)
            {
                if (entry.Value == playerId)
                    return entry.Key;
            }
        }

        return null;
    }

    // ---------------------------------------------------------
    // REGISTRY INITIALIZATION
    // ---------------------------------------------------------

    [CloudCodeFunction("InitializeRegistry")]
    public async Task InitializeRegistry(
        IExecutionContext context,
        IGameApiClient gameApiClient)
    {
        for (int shard = 0; shard < ShardCount; shard++)
        {
            string shardId =
                GetShardIdFromNumber(shard);

            var existing =
                await gameApiClient.CloudSaveData.GetPrivateCustomItemsAsync(
                    context,
                    context.ServiceToken,
                    context.ProjectId!,
                    shardId,
                    new List<string> { RegistryKey }
                );

            if (existing.Data.Results.Any())
                continue;

            await gameApiClient.CloudSaveData.SetPrivateCustomItemAsync(
                context,
                context.ServiceToken,
                context.ProjectId!,
                shardId,
                new SetItemBody(
                    RegistryKey,
                    "{}"
                )
            );
        }
    }

    // ---------------------------------------------------------
    // READ REGISTRY SHARD
    // ---------------------------------------------------------

    private async Task<RegistryShard> GetRegistryShard(
        IExecutionContext context,
        ICloudSaveDataApi cloudSave,
        string shardId)
    {
        var result =
            await cloudSave.GetPrivateCustomItemsAsync(
                context,
                context.ServiceToken,
                context.ProjectId!,
                shardId,
                new List<string> { RegistryKey }
            );

        var item =
            result.Data.Results.FirstOrDefault();

        if (item == null)
        {
            return new RegistryShard
            {
                Names = new Dictionary<string, string>(),
                WriteLock = null
            };
        }

        Dictionary<string, string> names;

        try
        {
            names =
                JsonSerializer.Deserialize<Dictionary<string, string>>(
                    item.Value?.ToString() ?? "{}"
                )
                ?? new Dictionary<string, string>();
        }
        catch
        {
            names =
                new Dictionary<string, string>();
        }

        return new RegistryShard
        {
            Names = names,
            WriteLock = item.WriteLock
        };
    }

    // ---------------------------------------------------------
    // NAME VALIDATION
    // ---------------------------------------------------------

    private static string ValidateAndCleanName(
        string requestedName)
    {
        if (string.IsNullOrWhiteSpace(requestedName))
            throw new Exception("Username cannot be empty.");

        string name =
            requestedName.Trim();

        if (name.Length < 1)
            throw new Exception(
                "Username is too short.");

        if (name.Length > 20)
            throw new Exception(
                "Username is too long.");

        if (name.Contains(" "))
            throw new Exception(
                "Username cannot contain spaces.");

        foreach (char character in name)
        {
            if (!char.IsLetterOrDigit(character) &&
                character != '_' &&
                character != '-')
            {
                throw new Exception(
                    "Username contains an invalid character.");
            }
        }

        return name;
    }

    // ---------------------------------------------------------
    // NORMALIZATION
    // ---------------------------------------------------------

    private static string NormalizeName(
        string name)
    {
        return name.Trim().ToLowerInvariant();
    }

    // ---------------------------------------------------------
    // SHARD CALCULATION
    // ---------------------------------------------------------

    private static string GetShardId(
        string normalizedName)
    {
        using SHA256 sha256 =
            SHA256.Create();

        byte[] hash =
            sha256.ComputeHash(
                Encoding.UTF8.GetBytes(
                    normalizedName));

        int shard =
            hash[0] % ShardCount;

        return GetShardIdFromNumber(shard);
    }

    private static string GetShardIdFromNumber(
        int shard)
    {
        return $"username-registry-{shard:D2}";
    }

    // ---------------------------------------------------------
    // INTERNAL DATA TYPES
    // ---------------------------------------------------------

    private class RegistryShard
    {
        public Dictionary<string, string> Names { get; set; }

        public string WriteLock { get; set; }
    }

    public class UsernameResult
    {
        public bool Success { get; set; }

        public string Username { get; set; }

        public string Error { get; set; }
    }

    // ---------------------------------------------------------
    // CLOUD CODE DEPENDENCY CONFIGURATION
    // ---------------------------------------------------------

    /*
    public class ModuleConfig : ICloudCodeSetup
    {
        public void Setup(
            ICloudCodeConfig config)
        {
            config.AddGameApiClient();
        }
    }
    
}

/*using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace HelloWorld;

public class MyModule
{
    readonly IGameApiClient m_GameApiClient;
    readonly ILogger<MyModule> m_Logger;

    public MyModule(IGameApiClient gameApiClient, ILogger<MyModule> logger)
    {
        m_GameApiClient = gameApiClient;
        m_Logger = logger;
    }

    [CloudCodeFunction("SayHello")]
    public string Hello(string name)
    {
        return $"Hello, {name}!";
    }

    [CloudCodeFunction("GetServerTime")]
    public string GetServerTime(IExecutionContext context)
    {
        return DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
    }
}*/


