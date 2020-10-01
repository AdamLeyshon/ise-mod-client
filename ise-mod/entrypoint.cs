using System;
using ise_core.db;
using LiteDB;
using MessagePack;
using UnityEngine;
using Verse;

namespace ise
{
    [StaticConstructorOnStartup]
    public class ISEBootStrap
    {
        [MessagePackObject]
        public class MyClass
        {
            // Key attributes take a serialization index (or string name)
            // The values must be unique and versioning has to be considered as well.
            // Keys are described in later sections in more detail.
            [Key(0)]
            public int Age { get; set; }

            [Key(1)]
            public string FirstName { get; set; }

            [Key(2)]
            public string LastName { get; set; }

            // All fields or properties that should not be serialized must be annotated with [IgnoreMember].
            [IgnoreMember]
            public string FullName { get { return FirstName + LastName; } }
        }
        
        static ISEBootStrap()
        {
            LoadOrCreateUserData();
            
            var mc = new MyClass
        {
            Age = 99,
            FirstName = "hoge",
            LastName = "huga",
        };

        // Call Serialize/Deserialize, that's all.
        byte[] bytes = MessagePackSerializer.Serialize(mc);
        MyClass mc2 = MessagePackSerializer.Deserialize<MyClass>(bytes);

        // You can dump MessagePack binary blobs to human readable json.
        // Using indexed keys (as opposed to string keys) will serialize to MessagePack arrays,
        // hence property names are not available.
        // [99,"hoge","huga"]
        var json = MessagePackSerializer.ToJson(bytes);
        Console.WriteLine(json);
        }

        // Normally we'd put this in the state tracker.
        // But I want to see if RW can actually run this.
        private static void LoadOrCreateUserData()
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"./ISEData.db"))
            {
                // Get the collection (or create, if doesn't exist)
                var col = db.GetCollection<DBUserData>("userdata");

                var existingUserData = col.FindById(1);

                if (existingUserData != null)
                {
                    Log.Message($"Loaded user ID: {existingUserData.UserId}");
                }

                // We need to create one when we first talk to the server.
                // For now let's just insert a test row.

                var customer = new DBUserData
                {
                    UserName = "Test User",
                    UserId = "12345ABCDE"
                };

                // Insert new customer document (Id will be auto-incremented)
                col.Insert(customer);
            }
        }
    }
}