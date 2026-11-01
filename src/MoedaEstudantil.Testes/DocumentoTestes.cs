using MoedaEstudantil.Dominio;

namespace MoedaEstudantil.Testes;

public class DocumentoTestes
{
    [Fact]
    public void Cpf_valido_aceita_digitos_e_mascara()
    {
        Assert.True(Documento.CpfValido("52998224725"));
        Assert.True(Documento.CpfValido("529.982.247-25"));
        Assert.Equal("529.982.247-25", Documento.FormatarCpf("52998224725"));
    }

    [Fact]
    public void Cpf_invalido_rejeita_sequencia_e_digito()
    {
        Assert.False(Documento.CpfValido("11111111111"));
        Assert.False(Documento.CpfValido("52998224726"));
        Assert.False(Documento.CpfValido("123"));
    }

    [Fact]
    public void Cnpj_conhecido_e_base_completam_os_digitos()
    {
        Assert.True(Documento.CnpjValido("00000000000191"));
        Assert.Equal("00000000000191", Documento.CompletarCnpj("000000000001"));
        Assert.Equal("00.000.000/0001-91", Documento.FormatarCnpj("00000000000191"));
        Assert.False(Documento.CnpjValido("00000000000100"));
    }
}
