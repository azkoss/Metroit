[English](README.md "English")

# Metroit #
���W�b�N���T�|�[�g���邢�����̃N���X�A�����WinForms�̊g���@�\�R���g���[���B  
�^�[�Q�b�g�t���[�����[�N��.NET 2.0, 4.5�ł��B  
WinForms�𗘗p�����J�����s���Ă�����ɂ����āA�������̎菕�������܂��B  
�悭���肻���ȓ���𔲂��o�����Ƃɂ��A�ʓ|�Ȏ������Ȃ��܂��B  
�悭���邱�Ƃ�AWPF�AEntityFramework�̗��p�ɐ��������鎞�ɁA�������������N���A�ɂ��Ă���邩������܂���B

## Metroit ##
��{�I�ȃN���X���܂܂�郉�C�u�����ł��B

#### �����݂���Dictionary ####
�Ӑ}�I�ɏ����K�v�Ƃ���Dictionary�𗘗p���܂��B
```C#
var dic = new Metroit.Collections.Generic.LimitedDictionary<string, string>(3);
dic.Add("key1", "value1");
dic.Add("key2", "value2");
dic.Add("key3", "value3");
if (!dic.CanAdd()) {
    dic.Add("key4", "value4");  // ArgumentException
}
```
#### string �N���X�̊g�� ####
```C#
using Metroit.Extensions;

// �啶���𔻒f�ɁA��؂蕶����}������B
var value = "TestTestTest";
Console.WriteLine(value.InsertSeparator("_", SeparateJudgeType.UpperChar));  // Test_Test_Test
```
#### �ۂߌv�Z���s�� ####
```C#
// �������ʂ��A3��4������B
var value = 1.24;
value = Metroit.MetMath.Round(1, 4, MidpointRounding.AwayFromZero); // 1.3

// ������O�ʂ�؂�グ��B
var value = 1.123;
value = Metroit.MetMath.Ceiling(1, 2); // 1.13

// ������O�ʂ�؂�̂Ă�B
var value = 1.123;
value = Metroit.MetMath.Floor(1, 2); // 1.12

// ������O�ʂ�؂�̂Ă�B
var value = 1.123;
value = Metroit.MetMath.Truncate(1, 2); // 1.12
```
#### �����̕ϊ����s�� ####
ConverterBase �N���X���g���āA������ϊ�����N���X���쐬���邱�Ƃ��ł��܂��B
```C#
using Metroit.IO;

class TestConverter : ConverterBase
{
    public TestConverter() :base()
    {
        Prepare += TestConverter_Prepare;
        ConvertCompleted += TestConverter_ConvertCompleted;
    }
    protected void DoConvert(IConverterParameter parameter)
    {
        // �Ȃɂ��̕ϊ�����
    }
    private void TestConverter_Prepare(IConvertParameter parameter, CancelEventArgs e)
    {
        // �ϊ��O��������
        // e.Cancel = true �ŕϊ��L�����Z��
    }
    private void TestConverter_ConvertCompleted(IConvertParameter parameter, ConvertCompleteEventArgs e)
    {
        // �ϊ���������
        switch (e.Result) {
            case ConvertResultType.Succeed:
                break;
            case ConvertResultType.Failed:
                Console.WriteLine(e.Error.ToString());
                break;
            case ConvertResultType.Cancelled:
                break;
        }
    }
}

class Test
{
    var converter = new TestConverter();
    var result = converter.Convert();
    if (result == ConvertResultType.Cancelled)
    {
        return;
    }
    // var t = converter.ConvertAsync(); // 4.5 only
}
```
#### �t�@�C���̕ϊ����s�� ####
FileConverterBase �N���X���g���āA�t�@�C����ϊ�����N���X���쐬���邱�Ƃ��ł��܂��B
```C#
using Metroit.IO;

class TestFileConverter : FileConverterBase
{
    public TestFileConverter() : base() { }

    protected void ConvertFileType() {
        // �t�@�C���̕ϊ������Ȃ�
        File.Copy(this.Parameter.SourceFilePath, this.Parameter.ConvertingPath);
    }
}

class Test
{
    var parameter = new FileConvertParameter()
    {
        SourceFilePath = "C:\test.txt",
        DestinationFilePath = "D:\test.dat",
        UseTemporary = true,
        Overwrite = true,

    };
    var converter = new TestFileConverter();
    var result = converter.Convert();
    if (result == ConvertResultType.Cancelled)
    {
        return;
    }
}
```
IFileConverterFactory, IFileConverterFactoryMetadata ���g���āA�ȒP��MEF���������邱�Ƃ��ł��܂��B
```C#
using Metroit.IO;

[Export(typeof(IFileTypeConverterFactory))]
[ExportMetadata("ConverterName", "XlsToPdf")]
[ExportMetadata("FromType", "xls")]
[ExportMetadata("ToType", "pdf")]
public class TestConverterFactory : IFileTypeConverterFactory
{
    public IO.FileTypeConverter Create()
    {
        return new TestConverter();
    }
}
```
---

## Metroit.Data ##
�f�[�^�x�[�X�����������@�\���܂܂�郉�C�u�����ł��B

#### �f�[�^�x�[�X�֐ڑ����� ####
�v���o�C�_�[�̕s�ϖ����o����K�v���Ȃ��Ȃ�܂��B  
Dictionary �ɂ��ڑ�����ݒ�ł��܂��B
```C#
using Metroit.Data.Common;
using Metroit.Data.Extensions;

var pf = MetDbProviderFactories.GetFactory(DatabaseType.Oracle);
using (var conn = pf.CreateConnection()) {
    conn.SetConnectionString(pf, new Dictionary<string, string>()
        {
            {"Data Source", "192.168.11.102/ORACLE" },
            {"User ID", "TEST" },
            {"Password","test" }
        });
    conn.Open();
}
```

#### �N�G���𔭍s���� ####
DbConnection.CreateQueryCommand()�́A�����I��BindByName()�����{���܂��B
```C#
// conn : DbConnection
var query = "SELECT * FROM TBL WHERE COLUMN1 = :COLUMN1 AND COLUMN2 = :COLUMN2";
var comm = conn.CreateQueryCommand(query);
// pf : ProviderFactiry
comm.Parameters.Add(pf.CreateParameter("COLUMN1", "value"));
comm.Parameters.Add(pf.CreateParameter("COLUMN2", "value"));
var dt = new DataTable();
comm.Fill(pf, dt);
```

#### �g�����U�N�V�����𗘗p�����N�G���𔭍s���� ####
DbTransaction.CreateQueryCommand()�́A�����I��BindByName()�����{���܂��B  
DbTransaction ���쐬��́ADbConnection �𑀍삷��K�v������܂���B
```C#
using Metroit.Data.Extensions;

// conn : DbConnection
using (var trans = conn.BeginTransaction())
{
    var query = "SELECT * FROM TBL WHERE COLUMN1 = :COLUMN1 AND COLUMN2 = :COLUMN2";
    var comm = trans.CreateQueryCommand(query);
    // pf : ProviderFactiry
    comm.Parameters.Add(pf.CreateParameter("COLUMN1", "value"));
    comm.Parameters.Add(pf.CreateParameter("COLUMN2", "value"));
    var dt = new DataTable();
    var da = comm.Fill(pf, dt, true, true);
    da.Update(dt);
    trans.Commit();
}
```

#### �v���V�[�W���̎��s���ʂ𓾂� ####
```C#
using Metroit.Data.Extensions;

// conn : DbConnection
var query = "ProcedureSample";
var comm = conn.CreateProcedureCommand(query);
// pf : ProviderFactiry
var parameter = pf.CreateParameter("COLUMN1", "value");
parameter.Direction = ParameterDirection.ReturnValue;
comm.Parameters.Add(pf);
comm.ExecuteNonQuery();
var result = comm.GetProcedureResult()
Console.WriteLine(result.ReturnValue.ToString());
```

#### �擾�����f�[�^���I�u�W�F�N�g�ő��삷�� ####
DataTable �𐶂ň������Ƃ�������܂��B
```C#
using Metroit.Data.Extensions;

class Tbl1
{
    public string Column1 { get; set; }

    // �v���p�e�B�� != �J�������̎��́AColumnAttribute �𗘗p����
    [Column("COLUMN2")]
    public string ColumnPrpoerty2 { get; set; }
}

var dt = new DataTable();
comm.Fill(pf, dt);

foreach(var row in dt.AsEnumerableEntity<Tbl1>())
{
    Console.WriteLine(row.Column1);
    Console.WriteLine(row.ColumnPrpoerty2);
}

// �s�P�ʂɍs���ꍇ
var row = dt[0].ToEntity<Tbl1>();
Console.WriteLine(row.Column1);
```

#### �N�G��������̍쐬���s�� ####
�N�G��������̐��������������܂��B
```C#
using Metroit.Data;

var builder = new QueryBuilder("SELECT *, /* REP */");
builder.AddQueries(new List<string>() { "FROM TBL "});
builder.ReplaceQueries(new List<string, string>() { "REP", "COLUMN1" });
var query = builder.Build(); // SELECT *, COLUMN1 FROM TBL
```

#### �N�G���p�����[�^�[�̍œK�����s�� ####
�N�G���Ɏw�肳��Ă���p�����[�^�[�̐ړ�����L�������߂���̂ɍœK�����܂��B  
�ړ�����":", "@"���A�L����"?"��F�����܂��B
```C#
using Metroit.Data;

var query = "SELECT * FROM TBL WHERE COLUMN1 = @COLUMN1";
var op = new QueryParameterOptimizer();
query = op.GetOptimizedText(query, QueryBindVariableType.ColonWithParam); // SELECT * FROM TBL WHERE COLUMN1 = :COLUMN1
```
---

## Metroit.Windows.Forms ##
WinForms �A�v���P�[�V�����̍쐬�������郉�C�u�����ł��B

#### �g�����ꂽ Form ####
- MetForm  
  ������UI����ƃ��W�b�N���菕�����܂��B
  - �v���p�e�B  

    |���O                |�Ӗ�                                                    |
    |--------------------|--------------------------------------------------------|
    |EnterFocus          |Enter�L�[�Ńt�H�[�J�X�J�ڂ��邩�ǂ����B                 |
    |EscPush             |ESC�L�[�̓���B                                         |
    |Request             |���N�G�X�g�f�[�^�B                                      |
    |Response            |���X�|���X�f�[�^�B                                      |

  - ���\�b�h  

    |���O                |�Ӗ�                                                    |
    |--------------------|--------------------------------------------------------|
    |Show                |���N�G�X�g����ʂɑ����ă��[�h���X�\������B            |
    |ShowDialog          |���N�G�X�g����ʂɑ����ă��[�_���\������B              |

#### �g�����ꂽ TextBox ####
- MetTextBox  
  ��������UI����ƃ��W�b�N���菕�����܂��B
  - �v���p�e�B  

    |���O                |�Ӗ�                                                    |
    |--------------------|--------------------------------------------------------|
    |AutoFocus           |�ő���͌��܂œ��͂��ꂽ��A���̃R���g���[���֑J�ڂ���B|
    |FocusSelect         |�t�H�[�J�X�𓾂����A�����𔽓]�����邩�ǂ����B          |
    |MultilineSelectAll  |Multiline�̎��ACtrl+A��L���ɂ���B                     |
    |BaseBackColor       |��{�̔w�i�F�B                                          |
    |BaseForeColor       |��{�̕����F�B                                          |
    |FocusBackColor      |�t�H�[�J�X�𓾂����̔w�i�F�B                            |
    |FocusForeColor      |�t�H�[�J�X�𓾂����̕����F�B                            |
    |ReadOnlyLabel       |Label �ɒu�������邩�ǂ����B                            |

        BackColor, ForeColor �́A���W�b�N����̂ݗ��p�\�ł��B  
        �������A�t�H�[�J�X�J�ڂɂ��ABaseBackColor, FocusBackColor, BaseForeColor, FocusForeColor ���D�悳��܂��B

  - �C�x���g  

    |���O                |�Ӗ�                                                    |
    |--------------------|--------------------------------------------------------|
    |TextChangeValidation|���͂��ꂽ�l�̎�����؂�����B                          |
    
        AutoComplete, ���ɖ߂��ɂ��ύX�͔������܂���B

- MetLimitedTextBox  
  MetTextBox ���p�����܂��B  
  �������͂̐�����K�v����ꍇ�̎菕�������܂��B  
  - �v���p�e�B  

    |���O                |�Ӗ�                                                    |
    |--------------------|--------------------------------------------------------|
    |AcceptsChar         |���͂��󂯓���镶���̎�ނ��w�肷��B                  |
    |ByteEncoding        |MaxByteLength�̔���ɗ��p���镶���G���R�[�f�B���O�B       |
    |CustomChars         |�J�X�^���w�莞�̎󂯕t���镶�����w�肷��B              |
    |ExcludeChars        |�����̎�ޓ��ŁA�󂯓���Ȃ��������w�肷��B            |
    |FullSignSpecialChars|�S�p�L���w�莞�ɁA���ɂ��󂯓����S�p�L�����w�肷��B  |
    |MaxByteLength       |���͂�������ő�o�C�g���B                            |

- MetNumericTextBox  
  MetTextBox ���p�����܂��B  
  ���l���͂̐�����K�v�Ƃ���ꍇ�̎菕�������܂��B
  - �v���p�e�B  

    |���O                |�Ӗ�                                                    |
    |--------------------|--------------------------------------------------------|
    |AcceptNegative      |�������󂯓���邩�ǂ����B                              |
    |AcceptNull          |null���󂯓���邩�ǂ����B                              |
    |CurrencySymbol      |���l�̕\�����@���ʉ݂̎��̋L���B                        |
    |DecimalDigits       |���͉\�ȏ��������B                                    |
    |DecimalSeparator    |�����Ə����̋�؂蕶���B                                |
    |GroupSeparator      |�����̋�؂蕶���B                                      |
    |GroupSizes          |�����̋�؂�ʒu�B                                      |
    |MaxValue            |���͂�������ő�l�B                                  |
    |MinValue            |���͂�������ŏ��l�B                                  |
    |NegativePattern     |�����̎��̕\�����@�B                                    |
    |NegativeSign        |�����̕\�������B                                        |
    |PercentSymbol       |���l�̕\�����@���p�[�Z���g�̎��̋L���B                  |
    |PositivePattern     |�����̎��̕\�����@�B                                    |
    |Mode                |���l�̕\�����@�B                                        |
    |NegativeForeColor   |�����̎��̕����F�B                                      |
    |Value               |���͒l�B                                                |

        ���L�̃v���p�e�B�͗��p�ł��܂���B  
        ImeMode, MaxLength, Multiline, PasswordChar, UseSystemPasswordChar, AcceptsReturn, AcceptsTab, CharacterCasing, Lines, ScrollBars, RightToLeft, MultilineSelectAll

#### �g�����ꂽ DateTimePicker ####
- MetDateTimePicker  
  ���t�̓��͂ɂ��āA�������̎菕�������܂��B
  - �v���p�e�B  

    |���O                |�Ӗ�                                                    |
    |--------------------|--------------------------------------------------------|
    |AcceptNull          |null���󂯓���邩�ǂ����B                              |
    |ReadOnly            |�ǂݎ���p�ɂ��邩�ǂ����B                            |
    |ReadOnlyLabel       |Label �ɒu�������邩�ǂ����B                            |
    |Value               |���͒l�B                                                |
    |BaseBackColor       |��{�̔w�i�F�B                                          |
    |BaseForeColor       |��{�̕����F�B                                          |
    |FocusBackColor      |�t�H�[�J�X�𓾂����̔w�i�F�B                            |
    |FocusForeColor      |�t�H�[�J�X�𓾂����̕����F�B                            |

        ReadOnly �́ATextBox �ɒu�������܂��B
