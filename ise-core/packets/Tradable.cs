// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: tradable.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Tradable {

  /// <summary>Holder for reflection information generated from tradable.proto</summary>
  public static partial class TradableReflection {

    #region Descriptor
    /// <summary>File descriptor for tradable.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TradableReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cg50cmFkYWJsZS5wcm90bxIIdHJhZGFibGUiuAEKCFRyYWRhYmxlEhAKCFRo",
            "aW5nRGVmGAEgASgJEhAKCEl0ZW1Db2RlGAUgASgJEg8KB1F1YWxpdHkYCiAB",
            "KAUSEAoIUXVhbnRpdHkYDyABKAUSEAoITWluaWZpZWQYFCABKAgSEQoJQmFz",
            "ZVZhbHVlGBkgASgCEg8KB1dlQnV5QXQYHiABKAISEAoIV2VTZWxsQXQYIyAB",
            "KAISDQoFU3R1ZmYYKCABKAkSDgoGV2VpZ2h0GC0gASgCIncKDkNvbG9ueVRy",
            "YWRhYmxlEhAKCFRoaW5nRGVmGAEgASgJEg8KB1F1YWxpdHkYCiABKAUSEAoI",
            "TWluaWZpZWQYFCABKAgSEQoJQmFzZVZhbHVlGBkgASgCEg4KBldlaWdodBge",
            "IAEoAhINCgVTdHVmZhgjIAEoCWIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Tradable.Tradable), global::Tradable.Tradable.Parser, new[]{ "ThingDef", "ItemCode", "Quality", "Quantity", "Minified", "BaseValue", "WeBuyAt", "WeSellAt", "Stuff", "Weight" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Tradable.ColonyTradable), global::Tradable.ColonyTradable.Parser, new[]{ "ThingDef", "Quality", "Minified", "BaseValue", "Weight", "Stuff" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class Tradable : pb::IMessage<Tradable>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<Tradable> _parser = new pb::MessageParser<Tradable>(() => new Tradable());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<Tradable> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tradable.TradableReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Tradable() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Tradable(Tradable other) : this() {
      thingDef_ = other.thingDef_;
      itemCode_ = other.itemCode_;
      quality_ = other.quality_;
      quantity_ = other.quantity_;
      minified_ = other.minified_;
      baseValue_ = other.baseValue_;
      weBuyAt_ = other.weBuyAt_;
      weSellAt_ = other.weSellAt_;
      stuff_ = other.stuff_;
      weight_ = other.weight_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Tradable Clone() {
      return new Tradable(this);
    }

    /// <summary>Field number for the "ThingDef" field.</summary>
    public const int ThingDefFieldNumber = 1;
    private string thingDef_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string ThingDef {
      get { return thingDef_; }
      set {
        thingDef_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "ItemCode" field.</summary>
    public const int ItemCodeFieldNumber = 5;
    private string itemCode_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string ItemCode {
      get { return itemCode_; }
      set {
        itemCode_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "Quality" field.</summary>
    public const int QualityFieldNumber = 10;
    private int quality_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int Quality {
      get { return quality_; }
      set {
        quality_ = value;
      }
    }

    /// <summary>Field number for the "Quantity" field.</summary>
    public const int QuantityFieldNumber = 15;
    private int quantity_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int Quantity {
      get { return quantity_; }
      set {
        quantity_ = value;
      }
    }

    /// <summary>Field number for the "Minified" field.</summary>
    public const int MinifiedFieldNumber = 20;
    private bool minified_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Minified {
      get { return minified_; }
      set {
        minified_ = value;
      }
    }

    /// <summary>Field number for the "BaseValue" field.</summary>
    public const int BaseValueFieldNumber = 25;
    private float baseValue_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float BaseValue {
      get { return baseValue_; }
      set {
        baseValue_ = value;
      }
    }

    /// <summary>Field number for the "WeBuyAt" field.</summary>
    public const int WeBuyAtFieldNumber = 30;
    private float weBuyAt_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float WeBuyAt {
      get { return weBuyAt_; }
      set {
        weBuyAt_ = value;
      }
    }

    /// <summary>Field number for the "WeSellAt" field.</summary>
    public const int WeSellAtFieldNumber = 35;
    private float weSellAt_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float WeSellAt {
      get { return weSellAt_; }
      set {
        weSellAt_ = value;
      }
    }

    /// <summary>Field number for the "Stuff" field.</summary>
    public const int StuffFieldNumber = 40;
    private string stuff_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Stuff {
      get { return stuff_; }
      set {
        stuff_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "Weight" field.</summary>
    public const int WeightFieldNumber = 45;
    private float weight_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Weight {
      get { return weight_; }
      set {
        weight_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as Tradable);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(Tradable other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ThingDef != other.ThingDef) return false;
      if (ItemCode != other.ItemCode) return false;
      if (Quality != other.Quality) return false;
      if (Quantity != other.Quantity) return false;
      if (Minified != other.Minified) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(BaseValue, other.BaseValue)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(WeBuyAt, other.WeBuyAt)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(WeSellAt, other.WeSellAt)) return false;
      if (Stuff != other.Stuff) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Weight, other.Weight)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (ThingDef.Length != 0) hash ^= ThingDef.GetHashCode();
      if (ItemCode.Length != 0) hash ^= ItemCode.GetHashCode();
      if (Quality != 0) hash ^= Quality.GetHashCode();
      if (Quantity != 0) hash ^= Quantity.GetHashCode();
      if (Minified != false) hash ^= Minified.GetHashCode();
      if (BaseValue != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(BaseValue);
      if (WeBuyAt != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(WeBuyAt);
      if (WeSellAt != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(WeSellAt);
      if (Stuff.Length != 0) hash ^= Stuff.GetHashCode();
      if (Weight != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Weight);
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (ThingDef.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ThingDef);
      }
      if (ItemCode.Length != 0) {
        output.WriteRawTag(42);
        output.WriteString(ItemCode);
      }
      if (Quality != 0) {
        output.WriteRawTag(80);
        output.WriteInt32(Quality);
      }
      if (Quantity != 0) {
        output.WriteRawTag(120);
        output.WriteInt32(Quantity);
      }
      if (Minified != false) {
        output.WriteRawTag(160, 1);
        output.WriteBool(Minified);
      }
      if (BaseValue != 0F) {
        output.WriteRawTag(205, 1);
        output.WriteFloat(BaseValue);
      }
      if (WeBuyAt != 0F) {
        output.WriteRawTag(245, 1);
        output.WriteFloat(WeBuyAt);
      }
      if (WeSellAt != 0F) {
        output.WriteRawTag(157, 2);
        output.WriteFloat(WeSellAt);
      }
      if (Stuff.Length != 0) {
        output.WriteRawTag(194, 2);
        output.WriteString(Stuff);
      }
      if (Weight != 0F) {
        output.WriteRawTag(237, 2);
        output.WriteFloat(Weight);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (ThingDef.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ThingDef);
      }
      if (ItemCode.Length != 0) {
        output.WriteRawTag(42);
        output.WriteString(ItemCode);
      }
      if (Quality != 0) {
        output.WriteRawTag(80);
        output.WriteInt32(Quality);
      }
      if (Quantity != 0) {
        output.WriteRawTag(120);
        output.WriteInt32(Quantity);
      }
      if (Minified != false) {
        output.WriteRawTag(160, 1);
        output.WriteBool(Minified);
      }
      if (BaseValue != 0F) {
        output.WriteRawTag(205, 1);
        output.WriteFloat(BaseValue);
      }
      if (WeBuyAt != 0F) {
        output.WriteRawTag(245, 1);
        output.WriteFloat(WeBuyAt);
      }
      if (WeSellAt != 0F) {
        output.WriteRawTag(157, 2);
        output.WriteFloat(WeSellAt);
      }
      if (Stuff.Length != 0) {
        output.WriteRawTag(194, 2);
        output.WriteString(Stuff);
      }
      if (Weight != 0F) {
        output.WriteRawTag(237, 2);
        output.WriteFloat(Weight);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (ThingDef.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ThingDef);
      }
      if (ItemCode.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ItemCode);
      }
      if (Quality != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Quality);
      }
      if (Quantity != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Quantity);
      }
      if (Minified != false) {
        size += 2 + 1;
      }
      if (BaseValue != 0F) {
        size += 2 + 4;
      }
      if (WeBuyAt != 0F) {
        size += 2 + 4;
      }
      if (WeSellAt != 0F) {
        size += 2 + 4;
      }
      if (Stuff.Length != 0) {
        size += 2 + pb::CodedOutputStream.ComputeStringSize(Stuff);
      }
      if (Weight != 0F) {
        size += 2 + 4;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(Tradable other) {
      if (other == null) {
        return;
      }
      if (other.ThingDef.Length != 0) {
        ThingDef = other.ThingDef;
      }
      if (other.ItemCode.Length != 0) {
        ItemCode = other.ItemCode;
      }
      if (other.Quality != 0) {
        Quality = other.Quality;
      }
      if (other.Quantity != 0) {
        Quantity = other.Quantity;
      }
      if (other.Minified != false) {
        Minified = other.Minified;
      }
      if (other.BaseValue != 0F) {
        BaseValue = other.BaseValue;
      }
      if (other.WeBuyAt != 0F) {
        WeBuyAt = other.WeBuyAt;
      }
      if (other.WeSellAt != 0F) {
        WeSellAt = other.WeSellAt;
      }
      if (other.Stuff.Length != 0) {
        Stuff = other.Stuff;
      }
      if (other.Weight != 0F) {
        Weight = other.Weight;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
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
            ThingDef = input.ReadString();
            break;
          }
          case 42: {
            ItemCode = input.ReadString();
            break;
          }
          case 80: {
            Quality = input.ReadInt32();
            break;
          }
          case 120: {
            Quantity = input.ReadInt32();
            break;
          }
          case 160: {
            Minified = input.ReadBool();
            break;
          }
          case 205: {
            BaseValue = input.ReadFloat();
            break;
          }
          case 245: {
            WeBuyAt = input.ReadFloat();
            break;
          }
          case 285: {
            WeSellAt = input.ReadFloat();
            break;
          }
          case 322: {
            Stuff = input.ReadString();
            break;
          }
          case 365: {
            Weight = input.ReadFloat();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            ThingDef = input.ReadString();
            break;
          }
          case 42: {
            ItemCode = input.ReadString();
            break;
          }
          case 80: {
            Quality = input.ReadInt32();
            break;
          }
          case 120: {
            Quantity = input.ReadInt32();
            break;
          }
          case 160: {
            Minified = input.ReadBool();
            break;
          }
          case 205: {
            BaseValue = input.ReadFloat();
            break;
          }
          case 245: {
            WeBuyAt = input.ReadFloat();
            break;
          }
          case 285: {
            WeSellAt = input.ReadFloat();
            break;
          }
          case 322: {
            Stuff = input.ReadString();
            break;
          }
          case 365: {
            Weight = input.ReadFloat();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class ColonyTradable : pb::IMessage<ColonyTradable>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<ColonyTradable> _parser = new pb::MessageParser<ColonyTradable>(() => new ColonyTradable());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<ColonyTradable> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tradable.TradableReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ColonyTradable() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ColonyTradable(ColonyTradable other) : this() {
      thingDef_ = other.thingDef_;
      quality_ = other.quality_;
      minified_ = other.minified_;
      baseValue_ = other.baseValue_;
      weight_ = other.weight_;
      stuff_ = other.stuff_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ColonyTradable Clone() {
      return new ColonyTradable(this);
    }

    /// <summary>Field number for the "ThingDef" field.</summary>
    public const int ThingDefFieldNumber = 1;
    private string thingDef_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string ThingDef {
      get { return thingDef_; }
      set {
        thingDef_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "Quality" field.</summary>
    public const int QualityFieldNumber = 10;
    private int quality_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int Quality {
      get { return quality_; }
      set {
        quality_ = value;
      }
    }

    /// <summary>Field number for the "Minified" field.</summary>
    public const int MinifiedFieldNumber = 20;
    private bool minified_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Minified {
      get { return minified_; }
      set {
        minified_ = value;
      }
    }

    /// <summary>Field number for the "BaseValue" field.</summary>
    public const int BaseValueFieldNumber = 25;
    private float baseValue_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float BaseValue {
      get { return baseValue_; }
      set {
        baseValue_ = value;
      }
    }

    /// <summary>Field number for the "Weight" field.</summary>
    public const int WeightFieldNumber = 30;
    private float weight_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Weight {
      get { return weight_; }
      set {
        weight_ = value;
      }
    }

    /// <summary>Field number for the "Stuff" field.</summary>
    public const int StuffFieldNumber = 35;
    private string stuff_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Stuff {
      get { return stuff_; }
      set {
        stuff_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as ColonyTradable);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(ColonyTradable other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ThingDef != other.ThingDef) return false;
      if (Quality != other.Quality) return false;
      if (Minified != other.Minified) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(BaseValue, other.BaseValue)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Weight, other.Weight)) return false;
      if (Stuff != other.Stuff) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (ThingDef.Length != 0) hash ^= ThingDef.GetHashCode();
      if (Quality != 0) hash ^= Quality.GetHashCode();
      if (Minified != false) hash ^= Minified.GetHashCode();
      if (BaseValue != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(BaseValue);
      if (Weight != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Weight);
      if (Stuff.Length != 0) hash ^= Stuff.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (ThingDef.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ThingDef);
      }
      if (Quality != 0) {
        output.WriteRawTag(80);
        output.WriteInt32(Quality);
      }
      if (Minified != false) {
        output.WriteRawTag(160, 1);
        output.WriteBool(Minified);
      }
      if (BaseValue != 0F) {
        output.WriteRawTag(205, 1);
        output.WriteFloat(BaseValue);
      }
      if (Weight != 0F) {
        output.WriteRawTag(245, 1);
        output.WriteFloat(Weight);
      }
      if (Stuff.Length != 0) {
        output.WriteRawTag(154, 2);
        output.WriteString(Stuff);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (ThingDef.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ThingDef);
      }
      if (Quality != 0) {
        output.WriteRawTag(80);
        output.WriteInt32(Quality);
      }
      if (Minified != false) {
        output.WriteRawTag(160, 1);
        output.WriteBool(Minified);
      }
      if (BaseValue != 0F) {
        output.WriteRawTag(205, 1);
        output.WriteFloat(BaseValue);
      }
      if (Weight != 0F) {
        output.WriteRawTag(245, 1);
        output.WriteFloat(Weight);
      }
      if (Stuff.Length != 0) {
        output.WriteRawTag(154, 2);
        output.WriteString(Stuff);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (ThingDef.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ThingDef);
      }
      if (Quality != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Quality);
      }
      if (Minified != false) {
        size += 2 + 1;
      }
      if (BaseValue != 0F) {
        size += 2 + 4;
      }
      if (Weight != 0F) {
        size += 2 + 4;
      }
      if (Stuff.Length != 0) {
        size += 2 + pb::CodedOutputStream.ComputeStringSize(Stuff);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(ColonyTradable other) {
      if (other == null) {
        return;
      }
      if (other.ThingDef.Length != 0) {
        ThingDef = other.ThingDef;
      }
      if (other.Quality != 0) {
        Quality = other.Quality;
      }
      if (other.Minified != false) {
        Minified = other.Minified;
      }
      if (other.BaseValue != 0F) {
        BaseValue = other.BaseValue;
      }
      if (other.Weight != 0F) {
        Weight = other.Weight;
      }
      if (other.Stuff.Length != 0) {
        Stuff = other.Stuff;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
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
            ThingDef = input.ReadString();
            break;
          }
          case 80: {
            Quality = input.ReadInt32();
            break;
          }
          case 160: {
            Minified = input.ReadBool();
            break;
          }
          case 205: {
            BaseValue = input.ReadFloat();
            break;
          }
          case 245: {
            Weight = input.ReadFloat();
            break;
          }
          case 282: {
            Stuff = input.ReadString();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            ThingDef = input.ReadString();
            break;
          }
          case 80: {
            Quality = input.ReadInt32();
            break;
          }
          case 160: {
            Minified = input.ReadBool();
            break;
          }
          case 205: {
            BaseValue = input.ReadFloat();
            break;
          }
          case 245: {
            Weight = input.ReadFloat();
            break;
          }
          case 282: {
            Stuff = input.ReadString();
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
