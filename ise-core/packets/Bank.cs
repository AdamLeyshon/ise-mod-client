// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: bank.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Bank {

  /// <summary>Holder for reflection information generated from bank.proto</summary>
  public static partial class BankReflection {

    #region Descriptor
    /// <summary>File descriptor for bank.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static BankReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CgpiYW5rLnByb3RvEgRiYW5rGgtvcmRlci5wcm90bxoMY29tbW9uLnByb3Rv",
            "InIKDUJhbmtEYXRhUmVwbHkSMQoHQmFsYW5jZRgBIAMoCzIgLmJhbmsuQmFu",
            "a0RhdGFSZXBseS5CYWxhbmNlRW50cnkaLgoMQmFsYW5jZUVudHJ5EgsKA2tl",
            "eRgBIAEoBRINCgV2YWx1ZRgCIAEoBToCOAEiOAoOQmFua0dldFJlcXVlc3QS",
            "FAoMQ2xpZW50QmluZElkGAEgASgJEhAKCENvbG9ueUlkGAIgASgJInUKE0Jh",
            "bmtXaXRoZHJhd1JlcXVlc3QSFAoMQ2xpZW50QmluZElkGAEgASgJEhAKCENv",
            "bG9ueUlkGAIgASgJEiYKCEN1cnJlbmN5GAMgASgOMhQuY29tbW9uLkN1cnJl",
            "bmN5RW51bRIOCgZBbW91bnQYBCABKAUidgoRQmFua1dpdGhkcmF3UmVwbHkS",
            "JQoERGF0YRgBIAEoCzIXLm9yZGVyLk9yZGVyU3RhdHVzUmVwbHkSKQoGU3Rh",
            "dHVzGAIgASgOMhkub3JkZXIuT3JkZXJSZXF1ZXN0U3RhdHVzEg8KB0JhbGFu",
            "Y2UYBSABKAUygwEKC0JhbmtTZXJ2aWNlEjIKA0dldBIULmJhbmsuQmFua0dl",
            "dFJlcXVlc3QaEy5iYW5rLkJhbmtEYXRhUmVwbHkiABJACghXaXRoZHJhdxIZ",
            "LmJhbmsuQmFua1dpdGhkcmF3UmVxdWVzdBoXLmJhbmsuQmFua1dpdGhkcmF3",
            "UmVwbHkiAGIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Order.OrderReflection.Descriptor, global::Common.CommonReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Bank.BankDataReply), global::Bank.BankDataReply.Parser, new[]{ "Balance" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { null, }),
            new pbr::GeneratedClrTypeInfo(typeof(global::Bank.BankGetRequest), global::Bank.BankGetRequest.Parser, new[]{ "ClientBindId", "ColonyId" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Bank.BankWithdrawRequest), global::Bank.BankWithdrawRequest.Parser, new[]{ "ClientBindId", "ColonyId", "Currency", "Amount" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Bank.BankWithdrawReply), global::Bank.BankWithdrawReply.Parser, new[]{ "Data", "Status", "Balance" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class BankDataReply : pb::IMessage<BankDataReply> {
    private static readonly pb::MessageParser<BankDataReply> _parser = new pb::MessageParser<BankDataReply>(() => new BankDataReply());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<BankDataReply> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Bank.BankReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BankDataReply() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BankDataReply(BankDataReply other) : this() {
      balance_ = other.balance_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BankDataReply Clone() {
      return new BankDataReply(this);
    }

    /// <summary>Field number for the "Balance" field.</summary>
    public const int BalanceFieldNumber = 1;
    private static readonly pbc::MapField<int, int>.Codec _map_balance_codec
        = new pbc::MapField<int, int>.Codec(pb::FieldCodec.ForInt32(8, 0), pb::FieldCodec.ForInt32(16, 0), 10);
    private readonly pbc::MapField<int, int> balance_ = new pbc::MapField<int, int>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::MapField<int, int> Balance {
      get { return balance_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as BankDataReply);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(BankDataReply other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!Balance.Equals(other.Balance)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= Balance.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      balance_.WriteTo(output, _map_balance_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += balance_.CalculateSize(_map_balance_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(BankDataReply other) {
      if (other == null) {
        return;
      }
      balance_.Add(other.balance_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            balance_.AddEntriesFrom(input, _map_balance_codec);
            break;
          }
        }
      }
    }

  }

  public sealed partial class BankGetRequest : pb::IMessage<BankGetRequest> {
    private static readonly pb::MessageParser<BankGetRequest> _parser = new pb::MessageParser<BankGetRequest>(() => new BankGetRequest());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<BankGetRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Bank.BankReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BankGetRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BankGetRequest(BankGetRequest other) : this() {
      clientBindId_ = other.clientBindId_;
      colonyId_ = other.colonyId_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BankGetRequest Clone() {
      return new BankGetRequest(this);
    }

    /// <summary>Field number for the "ClientBindId" field.</summary>
    public const int ClientBindIdFieldNumber = 1;
    private string clientBindId_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ClientBindId {
      get { return clientBindId_; }
      set {
        clientBindId_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "ColonyId" field.</summary>
    public const int ColonyIdFieldNumber = 2;
    private string colonyId_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ColonyId {
      get { return colonyId_; }
      set {
        colonyId_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as BankGetRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(BankGetRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ClientBindId != other.ClientBindId) return false;
      if (ColonyId != other.ColonyId) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ClientBindId.Length != 0) hash ^= ClientBindId.GetHashCode();
      if (ColonyId.Length != 0) hash ^= ColonyId.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (ClientBindId.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ClientBindId);
      }
      if (ColonyId.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(ColonyId);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ClientBindId.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ClientBindId);
      }
      if (ColonyId.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ColonyId);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(BankGetRequest other) {
      if (other == null) {
        return;
      }
      if (other.ClientBindId.Length != 0) {
        ClientBindId = other.ClientBindId;
      }
      if (other.ColonyId.Length != 0) {
        ColonyId = other.ColonyId;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            ClientBindId = input.ReadString();
            break;
          }
          case 18: {
            ColonyId = input.ReadString();
            break;
          }
        }
      }
    }

  }

  public sealed partial class BankWithdrawRequest : pb::IMessage<BankWithdrawRequest> {
    private static readonly pb::MessageParser<BankWithdrawRequest> _parser = new pb::MessageParser<BankWithdrawRequest>(() => new BankWithdrawRequest());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<BankWithdrawRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Bank.BankReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BankWithdrawRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BankWithdrawRequest(BankWithdrawRequest other) : this() {
      clientBindId_ = other.clientBindId_;
      colonyId_ = other.colonyId_;
      currency_ = other.currency_;
      amount_ = other.amount_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BankWithdrawRequest Clone() {
      return new BankWithdrawRequest(this);
    }

    /// <summary>Field number for the "ClientBindId" field.</summary>
    public const int ClientBindIdFieldNumber = 1;
    private string clientBindId_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ClientBindId {
      get { return clientBindId_; }
      set {
        clientBindId_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "ColonyId" field.</summary>
    public const int ColonyIdFieldNumber = 2;
    private string colonyId_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ColonyId {
      get { return colonyId_; }
      set {
        colonyId_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "Currency" field.</summary>
    public const int CurrencyFieldNumber = 3;
    private global::Common.CurrencyEnum currency_ = global::Common.CurrencyEnum.Utc;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Common.CurrencyEnum Currency {
      get { return currency_; }
      set {
        currency_ = value;
      }
    }

    /// <summary>Field number for the "Amount" field.</summary>
    public const int AmountFieldNumber = 4;
    private int amount_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Amount {
      get { return amount_; }
      set {
        amount_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as BankWithdrawRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(BankWithdrawRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ClientBindId != other.ClientBindId) return false;
      if (ColonyId != other.ColonyId) return false;
      if (Currency != other.Currency) return false;
      if (Amount != other.Amount) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ClientBindId.Length != 0) hash ^= ClientBindId.GetHashCode();
      if (ColonyId.Length != 0) hash ^= ColonyId.GetHashCode();
      if (Currency != global::Common.CurrencyEnum.Utc) hash ^= Currency.GetHashCode();
      if (Amount != 0) hash ^= Amount.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (ClientBindId.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ClientBindId);
      }
      if (ColonyId.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(ColonyId);
      }
      if (Currency != global::Common.CurrencyEnum.Utc) {
        output.WriteRawTag(24);
        output.WriteEnum((int) Currency);
      }
      if (Amount != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(Amount);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ClientBindId.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ClientBindId);
      }
      if (ColonyId.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ColonyId);
      }
      if (Currency != global::Common.CurrencyEnum.Utc) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Currency);
      }
      if (Amount != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Amount);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(BankWithdrawRequest other) {
      if (other == null) {
        return;
      }
      if (other.ClientBindId.Length != 0) {
        ClientBindId = other.ClientBindId;
      }
      if (other.ColonyId.Length != 0) {
        ColonyId = other.ColonyId;
      }
      if (other.Currency != global::Common.CurrencyEnum.Utc) {
        Currency = other.Currency;
      }
      if (other.Amount != 0) {
        Amount = other.Amount;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            ClientBindId = input.ReadString();
            break;
          }
          case 18: {
            ColonyId = input.ReadString();
            break;
          }
          case 24: {
            Currency = (global::Common.CurrencyEnum) input.ReadEnum();
            break;
          }
          case 32: {
            Amount = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  public sealed partial class BankWithdrawReply : pb::IMessage<BankWithdrawReply> {
    private static readonly pb::MessageParser<BankWithdrawReply> _parser = new pb::MessageParser<BankWithdrawReply>(() => new BankWithdrawReply());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<BankWithdrawReply> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Bank.BankReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BankWithdrawReply() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BankWithdrawReply(BankWithdrawReply other) : this() {
      data_ = other.data_ != null ? other.data_.Clone() : null;
      status_ = other.status_;
      balance_ = other.balance_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BankWithdrawReply Clone() {
      return new BankWithdrawReply(this);
    }

    /// <summary>Field number for the "Data" field.</summary>
    public const int DataFieldNumber = 1;
    private global::Order.OrderStatusReply data_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Order.OrderStatusReply Data {
      get { return data_; }
      set {
        data_ = value;
      }
    }

    /// <summary>Field number for the "Status" field.</summary>
    public const int StatusFieldNumber = 2;
    private global::Order.OrderRequestStatus status_ = global::Order.OrderRequestStatus.Rejected;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Order.OrderRequestStatus Status {
      get { return status_; }
      set {
        status_ = value;
      }
    }

    /// <summary>Field number for the "Balance" field.</summary>
    public const int BalanceFieldNumber = 5;
    private int balance_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Balance {
      get { return balance_; }
      set {
        balance_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as BankWithdrawReply);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(BankWithdrawReply other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Data, other.Data)) return false;
      if (Status != other.Status) return false;
      if (Balance != other.Balance) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (data_ != null) hash ^= Data.GetHashCode();
      if (Status != global::Order.OrderRequestStatus.Rejected) hash ^= Status.GetHashCode();
      if (Balance != 0) hash ^= Balance.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (data_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Data);
      }
      if (Status != global::Order.OrderRequestStatus.Rejected) {
        output.WriteRawTag(16);
        output.WriteEnum((int) Status);
      }
      if (Balance != 0) {
        output.WriteRawTag(40);
        output.WriteInt32(Balance);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (data_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Data);
      }
      if (Status != global::Order.OrderRequestStatus.Rejected) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Status);
      }
      if (Balance != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Balance);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(BankWithdrawReply other) {
      if (other == null) {
        return;
      }
      if (other.data_ != null) {
        if (data_ == null) {
          Data = new global::Order.OrderStatusReply();
        }
        Data.MergeFrom(other.Data);
      }
      if (other.Status != global::Order.OrderRequestStatus.Rejected) {
        Status = other.Status;
      }
      if (other.Balance != 0) {
        Balance = other.Balance;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (data_ == null) {
              Data = new global::Order.OrderStatusReply();
            }
            input.ReadMessage(Data);
            break;
          }
          case 16: {
            Status = (global::Order.OrderRequestStatus) input.ReadEnum();
            break;
          }
          case 40: {
            Balance = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
