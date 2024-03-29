using MySql.Data.MySqlClient.Properties;
using MySql.Data.Types;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Threading;

namespace MySql.Data.MySqlClient
{
    public sealed class MySqlDataReader : DbDataReader, IDataReader, IDisposable, IDataRecord
    {
        // Fields
        internal long affectedRows;
        private MySqlCommand command;
        private CommandBehavior commandBehavior;
        private MySqlConnection connection;
        private bool disableZeroAffectedRows;
        internal Driver driver;
        private bool isOpen = true;
        private ResultSet resultSet;
        private PreparableStatement statement;

        // Methods
        internal MySqlDataReader(MySqlCommand cmd, PreparableStatement statement, CommandBehavior behavior)
        {
            this.command = cmd;
            this.connection = this.command.Connection;
            this.commandBehavior = behavior;
            this.driver = this.connection.driver;
            this.affectedRows = -1L;
            this.statement = statement;
            if ((cmd.CommandType == CommandType.StoredProcedure) && (cmd.UpdatedRowSource == UpdateRowSource.FirstReturnedRecord))
            {
                this.disableZeroAffectedRows = true;
            }
        }

        private void AdjustOutputTypes()
        {
            for (int i = 0; i < this.FieldCount; i++)
            {
                string parameterName = this.GetName(i).Remove(0, "_cnet_param_".Length + 1);
                IMySqlValue iMySqlValue = MySqlField.GetIMySqlValue(this.command.Parameters.GetParameterFlexible(parameterName, true).MySqlDbType);
                if (iMySqlValue is MySqlBit)
                {
                    MySqlBit valueObject = (MySqlBit)iMySqlValue;
                    valueObject.ReadAsString = true;
                    this.resultSet.SetValueObject(i, valueObject);
                }
                else
                {
                    this.resultSet.SetValueObject(i, iMySqlValue);
                }
            }
        }

        private object ChangeType(IMySqlValue value, int fieldIndex, Type newType)
        {
            this.resultSet.Fields[fieldIndex].AddTypeConversion(newType);
            return Convert.ChangeType(value.Value, newType, CultureInfo.InvariantCulture);
        }

        private void ClearKillFlag()
        {
            string cmdText = "SELECT * FROM bogus_table LIMIT 0";
            MySqlCommand command = new MySqlCommand(cmdText, this.connection)
            {
                InternallyCreated = true
            };
            try
            {
                command.ExecuteReader();
            }
            catch (MySqlException exception)
            {
                if (exception.Number != 0x47a)
                {
                    throw;
                }
            }
        }

        public override void Close()
        {
            if (this.isOpen)
            {
                bool flag = (this.commandBehavior & CommandBehavior.CloseConnection) != CommandBehavior.Default;
                CommandBehavior commandBehavior = this.commandBehavior;
                try
                {
                    if (!commandBehavior.Equals(CommandBehavior.SchemaOnly))
                    {
                        this.commandBehavior = CommandBehavior.Default;
                    }
                    while (this.NextResult())
                    {
                    }
                }
                catch (MySqlException exception)
                {
                    if (!exception.IsQueryAborted)
                    {
                        bool flag2 = false;
                        for (Exception exception2 = exception; exception2 != null; exception2 = exception2.InnerException)
                        {
                            if (exception2 is IOException)
                            {
                                flag2 = true;
                                break;
                            }
                        }
                        if (!flag2)
                        {
                            throw;
                        }
                    }
                }
                catch (IOException)
                {
                }
                finally
                {
                    this.connection.Reader = null;
                    this.commandBehavior = commandBehavior;
                }
                this.command.Close(this);
                this.commandBehavior = CommandBehavior.Default;
                if (this.command.Canceled && this.connection.driver.Version.isAtLeast(5, 1, 0))
                {
                    this.ClearKillFlag();
                }
                if (flag)
                {
                    this.connection.Close();
                }
                this.command = null;
                this.connection.IsInUse = false;
                this.connection = null;
                this.isOpen = false;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }
        }

        ~MySqlDataReader()
        {
            this.Dispose(false);
        }

        public override bool GetBoolean(int i)
        {
            return Convert.ToBoolean(this.GetValue(i));
        }

        public bool GetBoolean(string name)
        {
            return this.GetBoolean(this.GetOrdinal(name));
        }

        public override byte GetByte(int i)
        {
            IMySqlValue fieldValue = this.GetFieldValue(i, false);
            if (fieldValue is MySqlUByte)
            {
                MySqlUByte num = (MySqlUByte)fieldValue;
                return num.Value;
            }
            MySqlByte num2 = (MySqlByte)fieldValue;
            return (byte)num2.Value;
        }

        public byte GetByte(string name)
        {
            return this.GetByte(this.GetOrdinal(name));
        }

        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            if (i >= this.FieldCount)
            {
                this.Throw(new IndexOutOfRangeException());
            }
            IMySqlValue fieldValue = this.GetFieldValue(i, false);
            if (!(fieldValue is MySqlBinary) && !(fieldValue is MySqlGuid))
            {
                this.Throw(new MySqlException("GetBytes can only be called on binary or guid columns"));
            }
            byte[] src = null;
            if (fieldValue is MySqlBinary)
            {
                MySqlBinary binary = (MySqlBinary)fieldValue;
                src = binary.Value;
            }
            else
            {
                MySqlGuid guid = (MySqlGuid)fieldValue;
                src = guid.Bytes;
            }
            if (buffer == null)
            {
                return (long)src.Length;
            }
            if ((bufferoffset >= buffer.Length) || (bufferoffset < 0))
            {
                this.Throw(new IndexOutOfRangeException("Buffer index must be a valid index in buffer"));
            }
            if (buffer.Length < (bufferoffset + length))
            {
                this.Throw(new ArgumentException("Buffer is not large enough to hold the requested data"));
            }
            if ((fieldOffset < 0L) || ((fieldOffset >= src.Length) && (src.Length > 0L)))
            {
                this.Throw(new IndexOutOfRangeException("Data index must be a valid index in the field"));
            }
            if (src.Length < (fieldOffset + length))
            {
                length = src.Length - ((int)fieldOffset);
            }
            Buffer.BlockCopy(src, (int)fieldOffset, buffer, bufferoffset, length);
            return (long)length;
        }

        public override char GetChar(int i)
        {
            return this.GetString(i)[0];
        }

        public char GetChar(string name)
        {
            return this.GetChar(this.GetOrdinal(name));
        }

        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            if (i >= this.FieldCount)
            {
                this.Throw(new IndexOutOfRangeException());
            }
            string str = this.GetString(i);
            if (buffer == null)
            {
                return (long)str.Length;
            }
            if ((bufferoffset >= buffer.Length) || (bufferoffset < 0))
            {
                this.Throw(new IndexOutOfRangeException("Buffer index must be a valid index in buffer"));
            }
            if (buffer.Length < (bufferoffset + length))
            {
                this.Throw(new ArgumentException("Buffer is not large enough to hold the requested data"));
            }
            if ((fieldoffset < 0L) || (fieldoffset >= str.Length))
            {
                this.Throw(new IndexOutOfRangeException("Field offset must be a valid index in the field"));
            }
            if (str.Length < length)
            {
                length = str.Length;
            }
            str.CopyTo((int)fieldoffset, buffer, bufferoffset, length);
            return (long)length;
        }

        public override string GetDataTypeName(int i)
        {
            if (!this.isOpen)
            {
                this.Throw(new Exception("No current query in data reader"));
            }
            if (i >= this.FieldCount)
            {
                this.Throw(new IndexOutOfRangeException());
            }
            IMySqlValue value2 = this.resultSet.Values[i];
            return value2.MySqlTypeName;
        }

        public override DateTime GetDateTime(int i)
        {
            MySqlDateTime time;
            IMySqlValue fieldValue = this.GetFieldValue(i, true);
            if (fieldValue is MySqlDateTime)
            {
                time = (MySqlDateTime)fieldValue;
            }
            else
            {
                time = MySqlDateTime.Parse(this.GetString(i));
            }
            time.TimezoneOffset = this.driver.timeZoneOffset;
            if (this.connection.Settings.ConvertZeroDateTime && !time.IsValidDateTime)
            {
                return DateTime.MinValue;
            }
            return time.GetDateTime();
        }

        public DateTime GetDateTime(string column)
        {
            return this.GetDateTime(this.GetOrdinal(column));
        }

        public override decimal GetDecimal(int i)
        {
            IMySqlValue fieldValue = this.GetFieldValue(i, true);
            if (fieldValue is MySqlDecimal)
            {
                MySqlDecimal num = (MySqlDecimal)fieldValue;
                return num.Value;
            }
            return Convert.ToDecimal(fieldValue.Value);
        }

        public decimal GetDecimal(string column)
        {
            return this.GetDecimal(this.GetOrdinal(column));
        }

        public override double GetDouble(int i)
        {
            IMySqlValue fieldValue = this.GetFieldValue(i, true);
            if (fieldValue is MySqlDouble)
            {
                MySqlDouble num = (MySqlDouble)fieldValue;
                return num.Value;
            }
            return Convert.ToDouble(fieldValue.Value);
        }

        public double GetDouble(string column)
        {
            return this.GetDouble(this.GetOrdinal(column));
        }

        public override IEnumerator GetEnumerator()
        {
            return new DbEnumerator(this, (this.commandBehavior & CommandBehavior.CloseConnection) != CommandBehavior.Default);
        }

        public override Type GetFieldType(int i)
        {
            if (!this.isOpen)
            {
                this.Throw(new Exception("No current query in data reader"));
            }
            if (i >= this.FieldCount)
            {
                this.Throw(new IndexOutOfRangeException());
            }
            IMySqlValue value2 = this.resultSet.Values[i];
            if (!(value2 is MySqlDateTime))
            {
                return value2.SystemType;
            }
            if (!this.connection.Settings.AllowZeroDateTime)
            {
                return typeof(DateTime);
            }
            return typeof(MySqlDateTime);
        }

        public Type GetFieldType(string column)
        {
            return this.GetFieldType(this.GetOrdinal(column));
        }

        private IMySqlValue GetFieldValue(int index, bool checkNull)
        {
            if ((index < 0) || (index >= this.FieldCount))
            {
                this.Throw(new ArgumentException(Resources.InvalidColumnOrdinal));
            }
            IMySqlValue value2 = this.resultSet[index];
            if (checkNull && value2.IsNull)
            {
                throw new SqlNullValueException();
            }
            return value2;
        }

        public override float GetFloat(int i)
        {
            IMySqlValue fieldValue = this.GetFieldValue(i, true);
            if (fieldValue is MySqlSingle)
            {
                MySqlSingle num = (MySqlSingle)fieldValue;
                return num.Value;
            }
            return Convert.ToSingle(fieldValue.Value);
        }

        public float GetFloat(string column)
        {
            return this.GetFloat(this.GetOrdinal(column));
        }

        public override Guid GetGuid(int i)
        {
            object obj2 = this.GetValue(i);
            if (obj2 is Guid)
            {
                return (Guid)obj2;
            }
            if (obj2 is string)
            {
                return new Guid(obj2 as string);
            }
            if (obj2 is byte[])
            {
                byte[] b = (byte[])obj2;
                if (b.Length == 0x10)
                {
                    return new Guid(b);
                }
            }
            this.Throw(new MySqlException(Resources.ValueNotSupportedForGuid));
            return Guid.Empty;
        }

        public Guid GetGuid(string column)
        {
            return this.GetGuid(this.GetOrdinal(column));
        }

        public override short GetInt16(int i)
        {
            IMySqlValue fieldValue = this.GetFieldValue(i, true);
            if (fieldValue is MySqlInt16)
            {
                MySqlInt16 num = (MySqlInt16)fieldValue;
                return num.Value;
            }
            return (short)this.ChangeType(fieldValue, i, typeof(short));
        }

        public short GetInt16(string column)
        {
            return this.GetInt16(this.GetOrdinal(column));
        }

        public override int GetInt32(int i)
        {
            IMySqlValue fieldValue = this.GetFieldValue(i, true);
            if (fieldValue is MySqlInt32)
            {
                MySqlInt32 num = (MySqlInt32)fieldValue;
                return num.Value;
            }
            return (int)this.ChangeType(fieldValue, i, typeof(int));
        }

        public int GetInt32(string column)
        {
            return this.GetInt32(this.GetOrdinal(column));
        }

        public override long GetInt64(int i)
        {
            IMySqlValue fieldValue = this.GetFieldValue(i, true);
            if (fieldValue is MySqlInt64)
            {
                MySqlInt64 num = (MySqlInt64)fieldValue;
                return num.Value;
            }
            return (long)this.ChangeType(fieldValue, i, typeof(long));
        }

        public long GetInt64(string column)
        {
            return this.GetInt64(this.GetOrdinal(column));
        }

        public MySqlDateTime GetMySqlDateTime(int column)
        {
            return (MySqlDateTime)this.GetFieldValue(column, true);
        }

        public MySqlDateTime GetMySqlDateTime(string column)
        {
            return this.GetMySqlDateTime(this.GetOrdinal(column));
        }

        public MySqlDecimal GetMySqlDecimal(int i)
        {
            return (MySqlDecimal)this.GetFieldValue(i, false);
        }

        public MySqlDecimal GetMySqlDecimal(string column)
        {
            return this.GetMySqlDecimal(this.GetOrdinal(column));
        }

        public MySqlGeometry GetMySqlGeometry(int i)
        {
            try
            {
                IMySqlValue fieldValue = this.GetFieldValue(i, false);
                if ((fieldValue is MySqlGeometry) || (fieldValue is MySqlBinary))
                {
                    return new MySqlGeometry(MySqlDbType.Geometry, (byte[])fieldValue.Value);
                }
            }
            catch
            {
                this.Throw(new Exception("Can't get MySqlGeometry from value"));
            }
            return new MySqlGeometry(true);
        }

        public MySqlGeometry GetMySqlGeometry(string column)
        {
            return this.GetMySqlGeometry(this.GetOrdinal(column));
        }

        public override string GetName(int i)
        {
            if (!this.isOpen)
            {
                this.Throw(new Exception("No current query in data reader"));
            }
            if (i >= this.FieldCount)
            {
                this.Throw(new IndexOutOfRangeException());
            }
            return this.resultSet.Fields[i].ColumnName;
        }

        public override int GetOrdinal(string name)
        {
            if (!this.isOpen || (this.resultSet == null))
            {
                this.Throw(new Exception("No current query in data reader"));
            }
            return this.resultSet.GetOrdinal(name);
        }

        public sbyte GetSByte(int i)
        {
            IMySqlValue fieldValue = this.GetFieldValue(i, false);
            if (fieldValue is MySqlByte)
            {
                MySqlByte num = (MySqlByte)fieldValue;
                return num.Value;
            }
            MySqlByte num2 = (MySqlByte)fieldValue;
            return num2.Value;
        }

        public sbyte GetSByte(string name)
        {
            return this.GetSByte(this.GetOrdinal(name));
        }

        public override DataTable GetSchemaTable()
        {
            if (this.FieldCount == 0)
            {
                return null;
            }
            DataTable table = new DataTable("SchemaTable");
            table.Columns.Add("ColumnName", typeof(string));
            table.Columns.Add("ColumnOrdinal", typeof(int));
            table.Columns.Add("ColumnSize", typeof(int));
            table.Columns.Add("NumericPrecision", typeof(int));
            table.Columns.Add("NumericScale", typeof(int));
            table.Columns.Add("IsUnique", typeof(bool));
            table.Columns.Add("IsKey", typeof(bool));
            DataColumn column = table.Columns["IsKey"];
            column.AllowDBNull = true;
            table.Columns.Add("BaseCatalogName", typeof(string));
            table.Columns.Add("BaseColumnName", typeof(string));
            table.Columns.Add("BaseSchemaName", typeof(string));
            table.Columns.Add("BaseTableName", typeof(string));
            table.Columns.Add("DataType", typeof(Type));
            table.Columns.Add("AllowDBNull", typeof(bool));
            table.Columns.Add("ProviderType", typeof(int));
            table.Columns.Add("IsAliased", typeof(bool));
            table.Columns.Add("IsExpression", typeof(bool));
            table.Columns.Add("IsIdentity", typeof(bool));
            table.Columns.Add("IsAutoIncrement", typeof(bool));
            table.Columns.Add("IsRowVersion", typeof(bool));
            table.Columns.Add("IsHidden", typeof(bool));
            table.Columns.Add("IsLong", typeof(bool));
            table.Columns.Add("IsReadOnly", typeof(bool));
            int num = 1;
            for (int i = 0; i < this.FieldCount; i++)
            {
                MySqlField field = this.resultSet.Fields[i];
                DataRow row = table.NewRow();
                row["ColumnName"] = field.ColumnName;
                row["ColumnOrdinal"] = num++;
                row["ColumnSize"] = field.IsTextField ? (field.ColumnLength / field.MaxLength) : field.ColumnLength;
                int precision = field.Precision;
                int scale = field.Scale;
                if (precision != -1)
                {
                    row["NumericPrecision"] = (short)precision;
                }
                if (scale != -1)
                {
                    row["NumericScale"] = (short)scale;
                }
                row["DataType"] = this.GetFieldType(i);
                row["ProviderType"] = (int)field.Type;
                row["IsLong"] = !field.IsBlob ? ((object)0) : ((object)(field.ColumnLength > 0xff));
                row["AllowDBNull"] = field.AllowsNull;
                row["IsReadOnly"] = false;
                row["IsRowVersion"] = false;
                row["IsUnique"] = false;
                row["IsKey"] = field.IsPrimaryKey;
                row["IsAutoIncrement"] = field.IsAutoIncrement;
                row["BaseSchemaName"] = field.DatabaseName;
                row["BaseCatalogName"] = null;
                row["BaseTableName"] = field.RealTableName;
                row["BaseColumnName"] = field.OriginalColumnName;
                table.Rows.Add(row);
            }
            return table;
        }

        public override string GetString(int i)
        {
            IMySqlValue fieldValue = this.GetFieldValue(i, true);
            if (fieldValue is MySqlBinary)
            {
                MySqlBinary binary = (MySqlBinary)fieldValue;
                byte[] bytes = binary.Value;
                return this.resultSet.Fields[i].Encoding.GetString(bytes, 0, bytes.Length);
            }
            return fieldValue.Value.ToString();
        }

        public string GetString(string column)
        {
            return this.GetString(this.GetOrdinal(column));
        }

        public TimeSpan GetTimeSpan(int column)
        {
            MySqlTimeSpan fieldValue = (MySqlTimeSpan)this.GetFieldValue(column, true);
            return fieldValue.Value;
        }

        public TimeSpan GetTimeSpan(string column)
        {
            return this.GetTimeSpan(this.GetOrdinal(column));
        }

        public ushort GetUInt16(int column)
        {
            IMySqlValue fieldValue = this.GetFieldValue(column, true);
            if (fieldValue is MySqlUInt16)
            {
                MySqlUInt16 num = (MySqlUInt16)fieldValue;
                return num.Value;
            }
            return (ushort)this.ChangeType(fieldValue, column, typeof(ushort));
        }

        public ushort GetUInt16(string column)
        {
            return this.GetUInt16(this.GetOrdinal(column));
        }

        public uint GetUInt32(int column)
        {
            IMySqlValue fieldValue = this.GetFieldValue(column, true);
            if (fieldValue is MySqlUInt32)
            {
                MySqlUInt32 num = (MySqlUInt32)fieldValue;
                return num.Value;
            }
            return (uint)this.ChangeType(fieldValue, column, typeof(uint));
        }

        public uint GetUInt32(string column)
        {
            return this.GetUInt32(this.GetOrdinal(column));
        }

        public ulong GetUInt64(int column)
        {
            IMySqlValue fieldValue = this.GetFieldValue(column, true);
            if (fieldValue is MySqlUInt64)
            {
                MySqlUInt64 num = (MySqlUInt64)fieldValue;
                return num.Value;
            }
            return (ulong)this.ChangeType(fieldValue, column, typeof(ulong));
        }

        public ulong GetUInt64(string column)
        {
            return this.GetUInt64(this.GetOrdinal(column));
        }

        public override object GetValue(int i)
        {
            if (!this.isOpen)
            {
                this.Throw(new Exception("No current query in data reader"));
            }
            if (i >= this.FieldCount)
            {
                this.Throw(new IndexOutOfRangeException());
            }
            IMySqlValue fieldValue = this.GetFieldValue(i, false);
            if (fieldValue.IsNull)
            {
                return DBNull.Value;
            }
            if (!(fieldValue is MySqlDateTime))
            {
                return fieldValue.Value;
            }
            MySqlDateTime time = (MySqlDateTime)fieldValue;
            if (!time.IsValidDateTime && this.connection.Settings.ConvertZeroDateTime)
            {
                return DateTime.MinValue;
            }
            if (this.connection.Settings.AllowZeroDateTime)
            {
                return fieldValue;
            }
            return time.GetDateTime();
        }

        public override int GetValues(object[] values)
        {
            int num = Math.Min(values.Length, this.FieldCount);
            for (int i = 0; i < num; i++)
            {
                values[i] = this.GetValue(i);
            }
            return num;
        }

        public override bool IsDBNull(int i)
        {
            return (DBNull.Value == this.GetValue(i));
        }

        public override bool NextResult()
        {
            bool flag2;
            if (!this.isOpen)
            {
                this.Throw(new MySqlException(Resources.NextResultIsClosed));
            }
            bool flag = ((this.command.CommandType == CommandType.TableDirect) && this.command.EnableCaching) && ((this.commandBehavior & CommandBehavior.SequentialAccess) == CommandBehavior.Default);
            if (this.resultSet != null)
            {
                this.resultSet.Close();
                if (flag)
                {
                    TableCache.AddToCache(this.command.CommandText, this.resultSet);
                }
            }
            if ((this.resultSet != null) && (((this.commandBehavior & CommandBehavior.SingleResult) != CommandBehavior.Default) || flag))
            {
                return false;
            }
            try
            {
                do
                {
                    this.resultSet = null;
                    if (flag)
                    {
                        this.resultSet = TableCache.RetrieveFromCache(this.command.CommandText, this.command.CacheAge);
                    }
                    if (this.resultSet == null)
                    {
                        this.resultSet = this.driver.NextResult(this.Statement.StatementId, false);
                        if (this.resultSet == null)
                        {
                            return false;
                        }
                        if (this.resultSet.IsOutputParameters && (this.command.CommandType == CommandType.StoredProcedure))
                        {
                            StoredProcedure statement = this.statement as StoredProcedure;
                            statement.ProcessOutputParameters(this);
                            this.resultSet.Close();
                            if (!statement.ServerProvidingOutputParameters)
                            {
                                return false;
                            }
                            this.resultSet = this.driver.NextResult(this.Statement.StatementId, true);
                        }
                        this.resultSet.Cached = flag;
                    }
                    if (this.resultSet.Size == 0)
                    {
                        this.Command.lastInsertedId = this.resultSet.InsertedId;
                        if (this.affectedRows == -1L)
                        {
                            this.affectedRows = this.resultSet.AffectedRows;
                        }
                        else
                        {
                            this.affectedRows += this.resultSet.AffectedRows;
                        }
                    }
                }
                while (this.resultSet.Size == 0);
                flag2 = true;
            }
            catch (MySqlException exception)
            {
                if (exception.IsFatal)
                {
                    this.connection.Abort();
                }
                if (exception.Number == 0)
                {
                    throw new MySqlException(Resources.FatalErrorReadingResult, exception);
                }
                if ((this.commandBehavior & CommandBehavior.CloseConnection) != CommandBehavior.Default)
                {
                    this.Close();
                }
                throw;
            }
            return flag2;
        }

        private void ProcessOutputParameters()
        {
            if (!this.driver.SupportsOutputParameters || !this.command.IsPrepared)
            {
                this.AdjustOutputTypes();
            }
            if ((this.commandBehavior & CommandBehavior.SchemaOnly) == CommandBehavior.Default)
            {
                this.resultSet.NextRow(this.commandBehavior);
                string str = "@_cnet_param_";
                for (int i = 0; i < this.FieldCount; i++)
                {
                    string name = this.GetName(i);
                    if (name.StartsWith(str))
                    {
                        name = name.Remove(0, str.Length);
                    }
                    this.command.Parameters.GetParameterFlexible(name, true).Value = this.GetValue(i);
                }
            }
        }

        public override bool Read()
        {
            bool flag;
            if (!this.isOpen)
            {
                this.Throw(new MySqlException("Invalid attempt to Read when reader is closed."));
            }
            if (this.resultSet == null)
            {
                return false;
            }
            try
            {
                flag = this.resultSet.NextRow(this.commandBehavior);
            }
            catch (TimeoutException exception)
            {
                this.connection.HandleTimeoutOrThreadAbort(exception);
                throw;
            }
            catch (ThreadAbortException exception2)
            {
                this.connection.HandleTimeoutOrThreadAbort(exception2);
                throw;
            }
            catch (MySqlException exception3)
            {
                if (exception3.IsFatal)
                {
                    this.connection.Abort();
                }
                if (exception3.IsQueryAborted)
                {
                    throw;
                }
                throw new MySqlException(Resources.FatalErrorDuringRead, exception3);
            }
            return flag;
        }

        IDataReader IDataRecord.GetData(int i)
        {
            return base.GetData(i);
        }

        private void Throw(Exception ex)
        {
            if (this.connection != null)
            {
                this.connection.Throw(ex);
            }
            throw ex;
        }

        // Properties
        internal MySqlCommand Command
        {
            get
            {
                return this.command;
            }
        }

        internal CommandBehavior CommandBehavior
        {
            get
            {
                return this.commandBehavior;
            }
        }

        public override int Depth
        {
            get
            {
                return 0;
            }
        }

        public override int FieldCount
        {
            get
            {
                if (this.resultSet != null)
                {
                    return this.resultSet.Size;
                }
                return 0;
            }
        }

        public override bool HasRows
        {
            get
            {
                return ((this.resultSet != null) && this.resultSet.HasRows);
            }
        }

        public override bool IsClosed
        {
            get
            {
                return !this.isOpen;
            }
        }

        public override object this[int i]
        {
            get
            {
                return this.GetValue(i);
            }
        }

        public override object this[string name]
        {
            get
            {
                return this[this.GetOrdinal(name)];
            }
        }

        public override int RecordsAffected
        {
            get
            {
                if (this.disableZeroAffectedRows && (this.affectedRows == 0L))
                {
                    return -1;
                }
                return (int)this.affectedRows;
            }
        }

        internal ResultSet ResultSet
        {
            get
            {
                return this.resultSet;
            }
        }

        internal PreparableStatement Statement
        {
            get
            {
                return this.statement;
            }
        }
    }
}
