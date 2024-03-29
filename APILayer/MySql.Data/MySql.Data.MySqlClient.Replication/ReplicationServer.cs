using System;

namespace MySql.Data.MySqlClient.Replication
{
	public class ReplicationServer
	{
		public string Name
		{
			get;
			private set;
		}

		public bool IsMaster
		{
			get;
			private set;
		}

		public string ConnectionString
		{
			get;
			private set;
		}

		public bool IsAvailable
		{
			get;
			set;
		}

		public ReplicationServer(string name, bool isMaster, string connectionString)
		{
			this.Name = name;
			this.IsMaster = isMaster;
			this.ConnectionString = connectionString;
			this.IsAvailable = true;
		}
	}
}
