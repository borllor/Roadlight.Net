using MySql.Data.MySqlClient.Properties;
using MySql.Data.Types;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;

namespace MySql.Data.MySqlClient
{
	internal class ISSchemaProvider : SchemaProvider
	{
		public ISSchemaProvider(MySqlConnection connection) : base(connection)
		{
		}

		protected override MySqlSchemaCollection GetCollections()
		{
			MySqlSchemaCollection collections = base.GetCollections();
			object[][] data = new object[][]
			{
				new object[]
				{
					"Views",
					2,
					3
				},
				new object[]
				{
					"ViewColumns",
					3,
					4
				},
				new object[]
				{
					"Procedure Parameters",
					5,
					1
				},
				new object[]
				{
					"Procedures",
					4,
					3
				},
				new object[]
				{
					"Triggers",
					2,
					4
				}
			};
			SchemaProvider.FillTable(collections, data);
			return collections;
		}

		protected override MySqlSchemaCollection GetRestrictions()
		{
			MySqlSchemaCollection restrictions = base.GetRestrictions();
			object[][] data = new object[][]
			{
				new object[]
				{
					"Procedure Parameters",
					"Database",
					"",
					0
				},
				new object[]
				{
					"Procedure Parameters",
					"Schema",
					"",
					1
				},
				new object[]
				{
					"Procedure Parameters",
					"Name",
					"",
					2
				},
				new object[]
				{
					"Procedure Parameters",
					"Type",
					"",
					3
				},
				new object[]
				{
					"Procedure Parameters",
					"Parameter",
					"",
					4
				},
				new object[]
				{
					"Procedures",
					"Database",
					"",
					0
				},
				new object[]
				{
					"Procedures",
					"Schema",
					"",
					1
				},
				new object[]
				{
					"Procedures",
					"Name",
					"",
					2
				},
				new object[]
				{
					"Procedures",
					"Type",
					"",
					3
				},
				new object[]
				{
					"Views",
					"Database",
					"",
					0
				},
				new object[]
				{
					"Views",
					"Schema",
					"",
					1
				},
				new object[]
				{
					"Views",
					"Table",
					"",
					2
				},
				new object[]
				{
					"ViewColumns",
					"Database",
					"",
					0
				},
				new object[]
				{
					"ViewColumns",
					"Schema",
					"",
					1
				},
				new object[]
				{
					"ViewColumns",
					"Table",
					"",
					2
				},
				new object[]
				{
					"ViewColumns",
					"Column",
					"",
					3
				},
				new object[]
				{
					"Triggers",
					"Database",
					"",
					0
				},
				new object[]
				{
					"Triggers",
					"Schema",
					"",
					1
				},
				new object[]
				{
					"Triggers",
					"Name",
					"",
					2
				},
				new object[]
				{
					"Triggers",
					"EventObjectTable",
					"",
					3
				}
			};
			SchemaProvider.FillTable(restrictions, data);
			return restrictions;
		}

		public override MySqlSchemaCollection GetDatabases(string[] restrictions)
		{
			MySqlSchemaCollection mySqlSchemaCollection = this.Query("SCHEMATA", "", new string[]
			{
				"SCHEMA_NAME"
			}, restrictions);
			mySqlSchemaCollection.Columns[1].Name = "database_name";
			mySqlSchemaCollection.Name = "Databases";
			return mySqlSchemaCollection;
		}

		public override MySqlSchemaCollection GetTables(string[] restrictions)
		{
			MySqlSchemaCollection mySqlSchemaCollection = this.Query("TABLES", "TABLE_TYPE != 'VIEW'", new string[]
			{
				"TABLE_CATALOG",
				"TABLE_SCHEMA",
				"TABLE_NAME",
				"TABLE_TYPE"
			}, restrictions);
			mySqlSchemaCollection.Name = "Tables";
			return mySqlSchemaCollection;
		}

		public override MySqlSchemaCollection GetColumns(string[] restrictions)
		{
			MySqlSchemaCollection mySqlSchemaCollection = this.Query("COLUMNS", null, new string[]
			{
				"TABLE_CATALOG",
				"TABLE_SCHEMA",
				"TABLE_NAME",
				"COLUMN_NAME"
			}, restrictions);
			mySqlSchemaCollection.RemoveColumn("CHARACTER_OCTET_LENGTH");
			mySqlSchemaCollection.Name = "Columns";
			base.QuoteDefaultValues(mySqlSchemaCollection);
			return mySqlSchemaCollection;
		}

		private MySqlSchemaCollection GetViews(string[] restrictions)
		{
			MySqlSchemaCollection mySqlSchemaCollection = this.Query("VIEWS", null, new string[]
			{
				"TABLE_CATALOG",
				"TABLE_SCHEMA",
				"TABLE_NAME"
			}, restrictions);
			mySqlSchemaCollection.Name = "Views";
			return mySqlSchemaCollection;
		}

		private MySqlSchemaCollection GetViewColumns(string[] restrictions)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder("SELECT C.* FROM information_schema.columns C");
			stringBuilder2.Append(" JOIN information_schema.views V ");
			stringBuilder2.Append("ON C.table_schema=V.table_schema AND C.table_name=V.table_name ");
			if (restrictions != null && restrictions.Length >= 2 && restrictions[1] != null)
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "C.table_schema='{0}' ", new object[]
				{
					restrictions[1]
				});
			}
			if (restrictions != null && restrictions.Length >= 3 && restrictions[2] != null)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append("AND ");
				}
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "C.table_name='{0}' ", new object[]
				{
					restrictions[2]
				});
			}
			if (restrictions != null && restrictions.Length == 4 && restrictions[3] != null)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append("AND ");
				}
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "C.column_name='{0}' ", new object[]
				{
					restrictions[3]
				});
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder2.AppendFormat(CultureInfo.InvariantCulture, " WHERE {0}", new object[]
				{
					stringBuilder
				});
			}
			MySqlSchemaCollection table = this.GetTable(stringBuilder2.ToString());
			table.Name = "ViewColumns";
			table.Columns[0].Name = "VIEW_CATALOG";
			table.Columns[1].Name = "VIEW_SCHEMA";
			table.Columns[2].Name = "VIEW_NAME";
			base.QuoteDefaultValues(table);
			return table;
		}

		private MySqlSchemaCollection GetTriggers(string[] restrictions)
		{
			MySqlSchemaCollection mySqlSchemaCollection = this.Query("TRIGGERS", null, new string[]
			{
				"TRIGGER_CATALOG",
				"TRIGGER_SCHEMA",
				"EVENT_OBJECT_TABLE",
				"TRIGGER_NAME"
			}, restrictions);
			mySqlSchemaCollection.Name = "Triggers";
			return mySqlSchemaCollection;
		}

		public override MySqlSchemaCollection GetProcedures(string[] restrictions)
		{
			try
			{
				if (this.connection.Settings.HasProcAccess)
				{
					return base.GetProcedures(restrictions);
				}
			}
			catch (MySqlException ex)
			{
				if (ex.Number != 1142)
				{
					throw;
				}
				this.connection.Settings.HasProcAccess = false;
			}
			MySqlSchemaCollection mySqlSchemaCollection = this.Query("ROUTINES", null, new string[]
			{
				"ROUTINE_CATALOG",
				"ROUTINE_SCHEMA",
				"ROUTINE_NAME",
				"ROUTINE_TYPE"
			}, restrictions);
			mySqlSchemaCollection.Name = "Procedures";
			return mySqlSchemaCollection;
		}

		private MySqlSchemaCollection GetProceduresWithParameters(string[] restrictions)
		{
			MySqlSchemaCollection procedures = this.GetProcedures(restrictions);
			procedures.AddColumn("ParameterList", typeof(string));
			foreach (MySqlSchemaRow current in procedures.Rows)
			{
				current["ParameterList"] = this.GetProcedureParameterLine(current);
			}
			return procedures;
		}

		private string GetProcedureParameterLine(MySqlSchemaRow isRow)
		{
			string text = "SHOW CREATE {0} `{1}`.`{2}`";
			text = string.Format(text, isRow["ROUTINE_TYPE"], isRow["ROUTINE_SCHEMA"], isRow["ROUTINE_NAME"]);
			MySqlCommand mySqlCommand = new MySqlCommand(text, this.connection);
			string result;
			using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
			{
				mySqlDataReader.Read();
				if (mySqlDataReader.IsDBNull(2))
				{
					result = null;
				}
				else
				{
					string @string = mySqlDataReader.GetString(1);
					string string2 = mySqlDataReader.GetString(2);
					MySqlTokenizer mySqlTokenizer = new MySqlTokenizer(string2);
					mySqlTokenizer.AnsiQuotes = (@string.IndexOf("ANSI_QUOTES") != -1);
					mySqlTokenizer.BackslashEscapes = (@string.IndexOf("NO_BACKSLASH_ESCAPES") == -1);
					string a = mySqlTokenizer.NextToken();
					while (a != "(")
					{
						a = mySqlTokenizer.NextToken();
					}
					int num = mySqlTokenizer.StartIndex + 1;
					a = mySqlTokenizer.NextToken();
					while (a != ")" || mySqlTokenizer.Quoted)
					{
						a = mySqlTokenizer.NextToken();
						if (a == "(" && !mySqlTokenizer.Quoted)
						{
							while (a != ")" || mySqlTokenizer.Quoted)
							{
								a = mySqlTokenizer.NextToken();
							}
							a = mySqlTokenizer.NextToken();
						}
					}
					result = string2.Substring(num, mySqlTokenizer.StartIndex - num);
				}
			}
			return result;
		}

		private MySqlSchemaCollection GetParametersForRoutineFromIS(string[] restrictions)
		{
			string[] keys = new string[]
			{
				"SPECIFIC_CATALOG",
				"SPECIFIC_SCHEMA",
				"SPECIFIC_NAME",
				"ROUTINE_TYPE",
				"PARAMETER_NAME"
			};
			StringBuilder stringBuilder = new StringBuilder("SELECT * FROM INFORMATION_SCHEMA.PARAMETERS");
			string whereClause = ISSchemaProvider.GetWhereClause(null, keys, restrictions);
			if (!string.IsNullOrEmpty(whereClause))
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " WHERE {0}", new object[]
				{
					whereClause
				});
			}
			MySqlSchemaCollection mySqlSchemaCollection = base.QueryCollection("parameters", stringBuilder.ToString());
			if (mySqlSchemaCollection.Rows.Count != 0 && (string)mySqlSchemaCollection.Rows[0]["routine_type"] == "FUNCTION")
			{
				mySqlSchemaCollection.Rows[0]["parameter_mode"] = "IN";
				mySqlSchemaCollection.Rows[0]["parameter_name"] = "return_value";
			}
			return mySqlSchemaCollection;
		}

		private MySqlSchemaCollection GetParametersFromIS(string[] restrictions, MySqlSchemaCollection routines)
		{
			MySqlSchemaCollection mySqlSchemaCollection = null;
			if (routines == null || routines.Rows.Count == 0)
			{
				if (restrictions == null)
				{
					mySqlSchemaCollection = base.QueryCollection("parameters", "SELECT * FROM INFORMATION_SCHEMA.PARAMETERS WHERE 1=2");
				}
				else
				{
					mySqlSchemaCollection = this.GetParametersForRoutineFromIS(restrictions);
				}
			}
			else
			{
				foreach (MySqlSchemaRow current in routines.Rows)
				{
					if (restrictions != null && restrictions.Length >= 3)
					{
						restrictions[2] = current["ROUTINE_NAME"].ToString();
					}
					mySqlSchemaCollection = this.GetParametersForRoutineFromIS(restrictions);
				}
			}
			mySqlSchemaCollection.Name = "Procedure Parameters";
			return mySqlSchemaCollection;
		}

		internal MySqlSchemaCollection CreateParametersTable()
		{
			MySqlSchemaCollection mySqlSchemaCollection = new MySqlSchemaCollection("Procedure Parameters");
			mySqlSchemaCollection.AddColumn("SPECIFIC_CATALOG", typeof(string));
			mySqlSchemaCollection.AddColumn("SPECIFIC_SCHEMA", typeof(string));
			mySqlSchemaCollection.AddColumn("SPECIFIC_NAME", typeof(string));
			mySqlSchemaCollection.AddColumn("ORDINAL_POSITION", typeof(int));
			mySqlSchemaCollection.AddColumn("PARAMETER_MODE", typeof(string));
			mySqlSchemaCollection.AddColumn("PARAMETER_NAME", typeof(string));
			mySqlSchemaCollection.AddColumn("DATA_TYPE", typeof(string));
			mySqlSchemaCollection.AddColumn("CHARACTER_MAXIMUM_LENGTH", typeof(int));
			mySqlSchemaCollection.AddColumn("CHARACTER_OCTET_LENGTH", typeof(int));
			mySqlSchemaCollection.AddColumn("NUMERIC_PRECISION", typeof(byte));
			mySqlSchemaCollection.AddColumn("NUMERIC_SCALE", typeof(int));
			mySqlSchemaCollection.AddColumn("CHARACTER_SET_NAME", typeof(string));
			mySqlSchemaCollection.AddColumn("COLLATION_NAME", typeof(string));
			mySqlSchemaCollection.AddColumn("DTD_IDENTIFIER", typeof(string));
			mySqlSchemaCollection.AddColumn("ROUTINE_TYPE", typeof(string));
			return mySqlSchemaCollection;
		}

		public virtual MySqlSchemaCollection GetProcedureParameters(string[] restrictions, MySqlSchemaCollection routines)
		{
			bool flag = this.connection.driver.Version.isAtLeast(5, 5, 3);
			MySqlSchemaCollection result;
			try
			{
				MySqlSchemaCollection mySqlSchemaCollection = this.CreateParametersTable();
				this.GetParametersFromShowCreate(mySqlSchemaCollection, restrictions, routines);
				result = mySqlSchemaCollection;
			}
			catch (Exception)
			{
				if (!flag)
				{
					throw;
				}
				result = this.GetParametersFromIS(restrictions, routines);
			}
			return result;
		}

		protected override MySqlSchemaCollection GetSchemaInternal(string collection, string[] restrictions)
		{
			MySqlSchemaCollection schemaInternal = base.GetSchemaInternal(collection, restrictions);
			if (schemaInternal != null)
			{
				return schemaInternal;
			}
			if (collection != null)
			{
				if (collection == "VIEWS")
				{
					return this.GetViews(restrictions);
				}
				if (collection == "PROCEDURES")
				{
					return this.GetProcedures(restrictions);
				}
				if (collection == "PROCEDURES WITH PARAMETERS")
				{
					return this.GetProceduresWithParameters(restrictions);
				}
				if (collection == "PROCEDURE PARAMETERS")
				{
					return this.GetProcedureParameters(restrictions, null);
				}
				if (collection == "TRIGGERS")
				{
					return this.GetTriggers(restrictions);
				}
				if (collection == "VIEWCOLUMNS")
				{
					return this.GetViewColumns(restrictions);
				}
			}
			return null;
		}

		private static string GetWhereClause(string initial_where, string[] keys, string[] values)
		{
			StringBuilder stringBuilder = new StringBuilder(initial_where);
			if (values != null)
			{
				int num = 0;
				while (num < keys.Length && num < values.Length)
				{
					if (values[num] != null && !(values[num] == string.Empty))
					{
						if (stringBuilder.Length > 0)
						{
							stringBuilder.Append(" AND ");
						}
						stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0} LIKE '{1}'", new object[]
						{
							keys[num],
							values[num]
						});
					}
					num++;
				}
			}
			return stringBuilder.ToString();
		}

		private MySqlSchemaCollection Query(string table_name, string initial_where, string[] keys, string[] values)
		{
			StringBuilder stringBuilder = new StringBuilder("SELECT * FROM INFORMATION_SCHEMA.");
			stringBuilder.Append(table_name);
			string whereClause = ISSchemaProvider.GetWhereClause(initial_where, keys, values);
			if (whereClause.Length > 0)
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " WHERE {0}", new object[]
				{
					whereClause
				});
			}
			return this.GetTable(stringBuilder.ToString());
		}

		private MySqlSchemaCollection GetTable(string sql)
		{
			MySqlSchemaCollection mySqlSchemaCollection = new MySqlSchemaCollection();
			MySqlCommand mySqlCommand = new MySqlCommand(sql, this.connection);
			MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			for (int i = 0; i < mySqlDataReader.FieldCount; i++)
			{
				mySqlSchemaCollection.AddColumn(mySqlDataReader.GetName(i), mySqlDataReader.GetFieldType(i));
			}
			using (mySqlDataReader)
			{
				while (mySqlDataReader.Read())
				{
					MySqlSchemaRow mySqlSchemaRow = mySqlSchemaCollection.AddRow();
					for (int j = 0; j < mySqlDataReader.FieldCount; j++)
					{
						mySqlSchemaRow[j] = mySqlDataReader.GetValue(j);
					}
				}
			}
			return mySqlSchemaCollection;
		}

		public override MySqlSchemaCollection GetForeignKeys(string[] restrictions)
		{
			if (!this.connection.driver.Version.isAtLeast(5, 1, 16))
			{
				return base.GetForeignKeys(restrictions);
			}
			string text = "SELECT rc.constraint_catalog, rc.constraint_schema,\r\n                rc.constraint_name, kcu.table_catalog, kcu.table_schema, rc.table_name,\r\n                rc.match_option, rc.update_rule, rc.delete_rule, \r\n                NULL as referenced_table_catalog,\r\n                kcu.referenced_table_schema, rc.referenced_table_name \r\n                FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc\r\n                LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu ON \r\n                kcu.constraint_catalog <=> rc.constraint_catalog AND\r\n                kcu.constraint_schema <=> rc.constraint_schema AND \r\n                kcu.constraint_name <=> rc.constraint_name AND\r\n                kcu.ORDINAL_POSITION=1 WHERE 1=1";
			StringBuilder stringBuilder = new StringBuilder();
			if (restrictions.Length >= 2 && !string.IsNullOrEmpty(restrictions[1]))
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " AND rc.constraint_schema LIKE '{0}'", new object[]
				{
					restrictions[1]
				});
			}
			if (restrictions.Length >= 3 && !string.IsNullOrEmpty(restrictions[2]))
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " AND rc.table_name LIKE '{0}'", new object[]
				{
					restrictions[2]
				});
			}
			if (restrictions.Length >= 4 && !string.IsNullOrEmpty(restrictions[3]))
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " AND rc.constraint_name LIKE '{0}'", new object[]
				{
					restrictions[2]
				});
			}
			text += stringBuilder.ToString();
			return this.GetTable(text);
		}

		public override MySqlSchemaCollection GetForeignKeyColumns(string[] restrictions)
		{
			if (!this.connection.driver.Version.isAtLeast(5, 0, 6))
			{
				return base.GetForeignKeyColumns(restrictions);
			}
			string text = "SELECT kcu.* FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu\r\n                WHERE kcu.referenced_table_name IS NOT NULL";
			StringBuilder stringBuilder = new StringBuilder();
			if (restrictions.Length >= 2 && !string.IsNullOrEmpty(restrictions[1]))
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " AND kcu.constraint_schema LIKE '{0}'", new object[]
				{
					restrictions[1]
				});
			}
			if (restrictions.Length >= 3 && !string.IsNullOrEmpty(restrictions[2]))
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " AND kcu.table_name LIKE '{0}'", new object[]
				{
					restrictions[2]
				});
			}
			if (restrictions.Length >= 4 && !string.IsNullOrEmpty(restrictions[3]))
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " AND kcu.constraint_name LIKE '{0}'", new object[]
				{
					restrictions[3]
				});
			}
			text += stringBuilder.ToString();
			return this.GetTable(text);
		}

		internal void GetParametersFromShowCreate(MySqlSchemaCollection parametersTable, string[] restrictions, MySqlSchemaCollection routines)
		{
			if (routines == null)
			{
				routines = this.GetSchema("procedures", restrictions);
			}
			MySqlCommand mySqlCommand = this.connection.CreateCommand();
			foreach (MySqlSchemaRow current in routines.Rows)
			{
				string commandText = string.Format("SHOW CREATE {0} `{1}`.`{2}`", current["ROUTINE_TYPE"], current["ROUTINE_SCHEMA"], current["ROUTINE_NAME"]);
				mySqlCommand.CommandText = commandText;
				try
				{
					string nameToRestrict = null;
					if (restrictions != null && restrictions.Length == 5 && restrictions[4] != null)
					{
						nameToRestrict = restrictions[4];
					}
					using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
					{
						mySqlDataReader.Read();
						string @string = mySqlDataReader.GetString(2);
						mySqlDataReader.Close();
						this.ParseProcedureBody(parametersTable, @string, current, nameToRestrict);
					}
				}
				catch (SqlNullValueException innerException)
				{
					throw new InvalidOperationException(string.Format(Resources.UnableToRetrieveParameters, current["ROUTINE_NAME"]), innerException);
				}
			}
		}

		private void ParseProcedureBody(MySqlSchemaCollection parametersTable, string body, MySqlSchemaRow row, string nameToRestrict)
		{
			List<string> list = new List<string>(new string[]
			{
				"IN",
				"OUT",
				"INOUT"
			});
			string text = row["SQL_MODE"].ToString();
			int num = 1;
			MySqlTokenizer mySqlTokenizer = new MySqlTokenizer(body);
			mySqlTokenizer.AnsiQuotes = (text.IndexOf("ANSI_QUOTES") != -1);
			mySqlTokenizer.BackslashEscapes = (text.IndexOf("NO_BACKSLASH_ESCAPES") == -1);
			mySqlTokenizer.ReturnComments = false;
			string text2 = mySqlTokenizer.NextToken();
			while (text2 != "(")
			{
				if (string.Compare(text2, "FUNCTION", StringComparison.OrdinalIgnoreCase) == 0 && nameToRestrict == null)
				{
					parametersTable.AddRow();
					ISSchemaProvider.InitParameterRow(row, parametersTable.Rows[0]);
				}
				text2 = mySqlTokenizer.NextToken();
			}
			text2 = mySqlTokenizer.NextToken();
			while (text2 != ")")
			{
				MySqlSchemaRow mySqlSchemaRow = parametersTable.NewRow();
				ISSchemaProvider.InitParameterRow(row, mySqlSchemaRow);
				mySqlSchemaRow["ORDINAL_POSITION"] = num++;
				string text3 = StringUtility.ToUpperInvariant(text2);
				if (!mySqlTokenizer.Quoted && list.Contains(text3))
				{
					mySqlSchemaRow["PARAMETER_MODE"] = text3;
					text2 = mySqlTokenizer.NextToken();
				}
				if (mySqlTokenizer.Quoted)
				{
					text2 = text2.Substring(1, text2.Length - 2);
				}
				mySqlSchemaRow["PARAMETER_NAME"] = text2;
				text2 = this.ParseDataType(mySqlSchemaRow, mySqlTokenizer);
				if (text2 == ",")
				{
					text2 = mySqlTokenizer.NextToken();
				}
				if (nameToRestrict == null || string.Compare(mySqlSchemaRow["PARAMETER_NAME"].ToString(), nameToRestrict, StringComparison.OrdinalIgnoreCase) == 0)
				{
					parametersTable.Rows.Add(mySqlSchemaRow);
				}
			}
			text2 = StringUtility.ToUpperInvariant(mySqlTokenizer.NextToken());
			if (string.Compare(text2, "RETURNS", StringComparison.OrdinalIgnoreCase) == 0)
			{
				MySqlSchemaRow mySqlSchemaRow2 = parametersTable.Rows[0];
				mySqlSchemaRow2["PARAMETER_NAME"] = "RETURN_VALUE";
				this.ParseDataType(mySqlSchemaRow2, mySqlTokenizer);
			}
		}

		private static void InitParameterRow(MySqlSchemaRow procedure, MySqlSchemaRow parameter)
		{
			parameter["SPECIFIC_CATALOG"] = null;
			parameter["SPECIFIC_SCHEMA"] = procedure["ROUTINE_SCHEMA"];
			parameter["SPECIFIC_NAME"] = procedure["ROUTINE_NAME"];
			parameter["PARAMETER_MODE"] = "IN";
			parameter["ORDINAL_POSITION"] = 0;
			parameter["ROUTINE_TYPE"] = procedure["ROUTINE_TYPE"];
		}

		private string ParseDataType(MySqlSchemaRow row, MySqlTokenizer tokenizer)
		{
			StringBuilder stringBuilder = new StringBuilder(StringUtility.ToUpperInvariant(tokenizer.NextToken()));
			row["DATA_TYPE"] = stringBuilder.ToString();
			string text = row["DATA_TYPE"].ToString();
			string text2 = tokenizer.NextToken();
			if (text2 == "(")
			{
				text2 = tokenizer.ReadParenthesis();
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", new object[]
				{
					text2
				});
				if (text != "ENUM" && text != "SET")
				{
					ISSchemaProvider.ParseDataTypeSize(row, text2);
				}
				text2 = tokenizer.NextToken();
			}
			else
			{
				stringBuilder.Append(ISSchemaProvider.GetDataTypeDefaults(text, row));
			}
			while (text2 != ")" && text2 != "," && string.Compare(text2, "begin", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(text2, "return", StringComparison.OrdinalIgnoreCase) != 0)
			{
				if (string.Compare(text2, "CHARACTER", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(text2, "BINARY", StringComparison.OrdinalIgnoreCase) != 0)
				{
					if (string.Compare(text2, "SET", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(text2, "CHARSET", StringComparison.OrdinalIgnoreCase) == 0)
					{
						row["CHARACTER_SET_NAME"] = tokenizer.NextToken();
					}
					else if (string.Compare(text2, "ASCII", StringComparison.OrdinalIgnoreCase) == 0)
					{
						row["CHARACTER_SET_NAME"] = "latin1";
					}
					else if (string.Compare(text2, "UNICODE", StringComparison.OrdinalIgnoreCase) == 0)
					{
						row["CHARACTER_SET_NAME"] = "ucs2";
					}
					else if (string.Compare(text2, "COLLATE", StringComparison.OrdinalIgnoreCase) == 0)
					{
						row["COLLATION_NAME"] = tokenizer.NextToken();
					}
					else
					{
						stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " {0}", new object[]
						{
							text2
						});
					}
				}
				text2 = tokenizer.NextToken();
			}
			if (stringBuilder.Length > 0)
			{
				row["DTD_IDENTIFIER"] = stringBuilder.ToString();
			}
			if (string.IsNullOrEmpty((string)row["COLLATION_NAME"]) && !string.IsNullOrEmpty((string)row["CHARACTER_SET_NAME"]))
			{
				row["COLLATION_NAME"] = CharSetMap.GetDefaultCollation(row["CHARACTER_SET_NAME"].ToString(), this.connection);
			}
			if (row["CHARACTER_MAXIMUM_LENGTH"] != null)
			{
				if (row["CHARACTER_SET_NAME"] == null)
				{
					row["CHARACTER_SET_NAME"] = "";
				}
				row["CHARACTER_OCTET_LENGTH"] = CharSetMap.GetMaxLength((string)row["CHARACTER_SET_NAME"], this.connection) * (int)row["CHARACTER_MAXIMUM_LENGTH"];
			}
			return text2;
		}

		private static string GetDataTypeDefaults(string type, MySqlSchemaRow row)
		{
			string format = "({0},{1})";
			object arg_11_0 = row["NUMERIC_PRECISION"];
			if (MetaData.IsNumericType(type) && string.IsNullOrEmpty((string)row["NUMERIC_PRECISION"]))
			{
				row["NUMERIC_PRECISION"] = 10;
				row["NUMERIC_SCALE"] = 0;
				if (!MetaData.SupportScale(type))
				{
					format = "({0})";
				}
				return string.Format(format, row["NUMERIC_PRECISION"], row["NUMERIC_SCALE"]);
			}
			return string.Empty;
		}

		private static void ParseDataTypeSize(MySqlSchemaRow row, string size)
		{
			size = size.Trim(new char[]
			{
				'(',
				')'
			});
			string[] array = size.Split(new char[]
			{
				','
			});
			if (!MetaData.IsNumericType(row["DATA_TYPE"].ToString()))
			{
				row["CHARACTER_MAXIMUM_LENGTH"] = int.Parse(array[0]);
				return;
			}
			row["NUMERIC_PRECISION"] = int.Parse(array[0]);
			if (array.Length == 2)
			{
				row["NUMERIC_SCALE"] = int.Parse(array[1]);
			}
		}
	}
}
