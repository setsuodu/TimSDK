using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SQLite4Unity3d;

public class DataService
{
	private SQLiteConnection _connection;

	public DataService(string DatabaseName)
	{
        // check if file exists in Application.persistentDataPath
        var filepath = $"{Application.persistentDataPath}/{DatabaseName}";

        if (!File.Exists(filepath))
        {
            Debug.Log("Database not in Persistent path");
            // if it doesn't ->
            // open StreamingAssets directory and load the db ->

#if UNITY_ANDROID
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + DatabaseName);  // this is the path to your StreamingAssets in android
            while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
            // then save to Application.persistentDataPath
            File.WriteAllBytes(filepath, loadDb.bytes);
#elif UNITY_IOS
			var loadDb = Application.dataPath + "/Raw/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
			// then save to Application.persistentDataPath
			File.Copy(loadDb, filepath);
#elif UNITY_STANDALONE_OSX
			var loadDb = Application.dataPath + "/Resources/Data/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
			// then save to Application.persistentDataPath
			File.Copy(loadDb, filepath);
#else
			var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
			// then save to Application.persistentDataPath
			File.Copy(loadDb, filepath);
#endif
            Debug.Log("Database written");
        }

        var dbPath = filepath;

		_connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
		Debug.Log("Final PATH: " + dbPath);
	}

	public void CreateDB()
	{
		_connection.DropTable<Person>();
		_connection.CreateTable<Person>();
		_connection.InsertAll(new[]{
			new Person{
				Id = 1,
				Name = "Tom",
				Surname = "Perez",
				Age = 56
			},
			new Person{
				Id = 2,
				Name = "Fred",
				Surname = "Arthurson",
				Age = 16
			},
			new Person{
				Id = 3,
				Name = "John",
				Surname = "Doe",
				Age = 25
			},
			new Person{
				Id = 4,
				Name = "Roberto",
				Surname = "Huertas",
				Age = 37
			}
		});
	}

	public IEnumerable<Person> GetPersons()
	{
		return _connection.Table<Person>();
	}

	public IEnumerable<Person> GetPersonsNamedRoberto()
	{
		return _connection.Table<Person>().Where(x => x.Name == "Roberto");
	}

	public Person GetJohnny()
	{
		return _connection.Table<Person>().Where(x => x.Name == "Johnny").FirstOrDefault();
	}

	public Person CreatePerson()
	{
		var p = new Person
		{
			Name = "Johnny",
			Surname = "Mnemonic",
			Age = 21
		};
		_connection.Insert(p);
		return p;
	}

	//

	public void Close()
	{
		_connection.Close();
		_connection.Dispose();
		_connection = null;
	}

	public void CreateTable<T>()
	{
		var columns = _connection.GetTableInfo($"{typeof(T)}"); //字段
		//Debug.Log(columns.Count);
		if (columns.Count == 0)
			_connection.CreateTable<T>();
	}

	// 增
	public TIMUserProfileExt InsertProfile(TIMUserProfileExt _profile)
	{
		int result = _connection.Insert(_profile);
		Debug.Log($"Insert code={result}");
		return _profile;
	}

	// 删
	public void DeleteProfile(string _identifier)
	{
		int result = _connection.Delete<TIMUserProfileExt>("2");
		Debug.Log($"Delete code={result}");
	}

	// 改
	public TIMUserProfileExt UpdateProfile(string _identifier, string _nickName)
	{
		var prof = _connection.Table<TIMUserProfileExt>().Where(x => x.identifier == _identifier).FirstOrDefault();
		prof.nickName = _nickName;
		int result = _connection.Update(prof);
		Debug.Log($"Update code={result}");
		return prof;
	}

	// 查
	public TIMUserProfileExt QueryProfile(string _identifier)
	{
		return _connection.Table<TIMUserProfileExt>().Where(x => x.identifier == _identifier).FirstOrDefault();
	}
}
