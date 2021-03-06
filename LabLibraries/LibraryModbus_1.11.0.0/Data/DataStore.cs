using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Modbus.Utility;

namespace Modbus.Data
{
	/// <summary>
	/// Object simulation of device memory map.
	/// The underlying collections are thread safe when using the ModbusMaster API to read/write values.
	/// You can use the SyncRoot property to synchronize direct access to the DataStore collections.
	/// </summary>
	public class DataStore
	{
		/// <summary>
		/// Occurs when the DataStore is read from via a Modbus command.
		/// </summary>
		public event EventHandler<DataStoreEventArgs> DataStoreReadFrom;

		/// <summary>
		/// Occurs when the DataStore is written to via a Modbus command.
		/// </summary>		
		public event EventHandler<DataStoreEventArgs> DataStoreWrittenTo;

		private readonly object _syncRoot = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="DataStore"/> class.
		/// </summary>
		public DataStore()
		{
			CoilDiscretes = new ModbusDataCollection<bool> { ModbusDataType = ModbusDataType.Coil };
			InputDiscretes = new ModbusDataCollection<bool> { ModbusDataType = ModbusDataType.Input };
			HoldingRegisters = new ModbusDataCollection<ushort> { ModbusDataType = ModbusDataType.HoldingRegister };
			InputRegisters = new ModbusDataCollection<ushort> { ModbusDataType = ModbusDataType.InputRegister };
		}

		/// <summary>
		/// Gets the coil discretes.
		/// </summary>
		public ModbusDataCollection<bool> CoilDiscretes { get; private set; }

		/// <summary>
		/// Gets the input discretes.
		/// </summary>
		public ModbusDataCollection<bool> InputDiscretes { get; private set; }

		/// <summary>
		/// Gets the holding registers.
		/// </summary>
		public ModbusDataCollection<ushort> HoldingRegisters { get; private set; }

		/// <summary>
		/// Gets the input registers.
		/// </summary>
		public ModbusDataCollection<ushort> InputRegisters { get; private set; }

		/// <summary>
		/// An object that can be used to synchronize direct access to the DataStore collections.
		/// </summary>
		public Object SyncRoot
		{
			get
			{
				return _syncRoot;
			}
		}

		/// <summary>
		/// Retrieves subset of data from collection.
		/// </summary>
		/// <typeparam name="T">The collection type.</typeparam>
		/// <typeparam name="U">The type of elements in the collection.</typeparam>
		internal static T ReadData<T, U>(DataStore dataStore, ModbusDataCollection<U> dataSource, ushort startAddress, ushort count, object syncRoot) where T : Collection<U>, new()
		{
			int startIndex = startAddress + 1;

			if (startIndex < 0 || startIndex >= dataSource.Count)
				throw new ArgumentOutOfRangeException("Start address was out of range. Must be non-negative and <= the size of the collection.");

			if (dataSource.Count < startIndex + count)
				throw new ArgumentOutOfRangeException("Read is outside valid range.");
			
			U[] dataToRetrieve;
			lock(syncRoot)
				dataToRetrieve = CollectionUtility.Slice(dataSource, startIndex, count);
			
			T result = new T();
			for (int i = 0; i < count; i++)
				result.Add(dataToRetrieve[i]);

            EventHandler<DataStoreEventArgs> handler = dataStore.DataStoreReadFrom;
			if (handler != null)
                handler(dataStore, DataStoreEventArgs.CreateDataStoreEventArgs(startAddress, dataSource.ModbusDataType, result));

			return result;
		}

		/// <summary>
		/// Write data to data store.
		/// </summary>
		/// <typeparam name="TData">The type of the data.</typeparam>
		internal static void WriteData<TData>(DataStore dataStore, IList<TData> items, ModbusDataCollection<TData> destination, ushort startAddress, object syncRoot)
		{
			int startIndex = startAddress + 1;

			if (startIndex < 0 || startIndex >= destination.Count)
				throw new ArgumentOutOfRangeException("Start address was out of range. Must be non-negative and <= the size of the collection.");

			if (destination.Count < startIndex + items.Count)
				throw new ArgumentOutOfRangeException("Items collection is too large to write at specified start index.");

			lock (syncRoot)
				CollectionUtility.Update(items, destination, startIndex);

			EventHandler<DataStoreEventArgs> handler = dataStore.DataStoreWrittenTo;
            if (handler != null)
                handler(dataStore, DataStoreEventArgs.CreateDataStoreEventArgs(startAddress, destination.ModbusDataType, items));
		}
	}
}
