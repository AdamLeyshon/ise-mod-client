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
            "CgpiYW5rLnByb3RvEgRiYW5rInIKDUJhbmtEYXRhUmVwbHkSMQoHYmFsYW5j",
            "ZRgBIAMoCzIgLmJhbmsuQmFua0RhdGFSZXBseS5CYWxhbmNlRW50cnkaLgoM",
            "QmFsYW5jZUVudHJ5EgsKA2tleRgBIAEoBRINCgV2YWx1ZRgCIAEoBToCOAEi",
            "OAoOQmFua0dldFJlcXVlc3QSFAoMQ2xpZW50QmluZElkGAEgASgJEhAKCENv",
            "bG9ueUlkGAIgASgJKhcKDEJhbmtDdXJyZW5jeRIHCgNVVEMQADJBCgtCYW5r",
            "U2VydmljZRIyCgNHZXQSFC5iYW5rLkJhbmtHZXRSZXF1ZXN0GhMuYmFuay5C",
            "YW5rRGF0YVJlcGx5IgBiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::Bank.BankCurrency), }, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Bank.BankDataReply), global::Bank.BankDataReply.Parser, new[]{ "Balance" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { null, }),
            new pbr::GeneratedClrTypeInfo(typeof(global::Bank.BankGetRequest), global::Bank.BankGetRequest.Parser, new[]{ "ClientBindId", "ColonyId" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  public enum BankCurrency {
    /// <summary>
    /// Universal Trade Credits ᛊ
    /// </summary>
    [pbr::OriginalName("UTC")] Utc = 0,
  }

  #endregion

  #region Messages
  public sealed partial class BankDataReply : pb::IMessage<BankDataReply>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
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

    /// <summary>Field number for the "balance" field.</summary>
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
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      balance_.WriteTo(output, _map_balance_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      balance_.WriteTo(ref output, _map_balance_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

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
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
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
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            balance_.AddEntriesFrom(ref input, _map_balance_codec);
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class BankGetRequest : pb::IMessage<BankGetRequest>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
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
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
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
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (ClientBindId.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ClientBindId);
      }
      if (ColonyId.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(ColonyId);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

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
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
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
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
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
    #endif

  }

  #endregion

}

#endregion Designer generated code
