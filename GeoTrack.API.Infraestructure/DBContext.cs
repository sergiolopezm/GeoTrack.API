using Microsoft.EntityFrameworkCore;

namespace GeoTrack.API.Infrastructure;

public partial class DBContext : DbContext
{
    public DBContext()
    {
    }

    public DBContext(DbContextOptions<DBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Acceso> Accesos { get; set; }

    public virtual DbSet<Ciudade> Ciudades { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Paise> Paises { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<TokensExpirado> TokensExpirados { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Acceso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Accesos__3214EC07B4A8828D");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Contraseña).HasMaxLength(250);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Sitio).HasMaxLength(50);
        });

        modelBuilder.Entity<Ciudade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ciudades__3214EC07822001AD");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.CodigoPostal).HasMaxLength(20);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(100);

            entity.HasOne(d => d.CreadoPor).WithMany(p => p.CiudadeCreadoPors)
                .HasForeignKey(d => d.CreadoPorId)
                .HasConstraintName("FK__Ciudades__Creado__6477ECF3");

            entity.HasOne(d => d.Departamento).WithMany(p => p.Ciudades)
                .HasForeignKey(d => d.DepartamentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ciudades__Depart__6383C8BA");

            entity.HasOne(d => d.ModificadoPor).WithMany(p => p.CiudadeModificadoPors)
                .HasForeignKey(d => d.ModificadoPorId)
                .HasConstraintName("FK__Ciudades__Modifi__656C112C");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departam__3214EC0770AB8FE9");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(100);

            entity.HasOne(d => d.CreadoPor).WithMany(p => p.DepartamentoCreadoPors)
                .HasForeignKey(d => d.CreadoPorId)
                .HasConstraintName("FK__Departame__Cread__5DCAEF64");

            entity.HasOne(d => d.ModificadoPor).WithMany(p => p.DepartamentoModificadoPors)
                .HasForeignKey(d => d.ModificadoPorId)
                .HasConstraintName("FK__Departame__Modif__5EBF139D");

            entity.HasOne(d => d.Pais).WithMany(p => p.Departamentos)
                .HasForeignKey(d => d.PaisId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Departame__PaisI__5CD6CB2B");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Logs__3214EC07D3F9A35A");

            entity.Property(e => e.Accion).HasMaxLength(200);
            entity.Property(e => e.Fecha).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Ip).HasMaxLength(45);
            entity.Property(e => e.Tipo).HasMaxLength(50);

            entity.HasOne(d => d.Usuario).WithMany(p => p.Logs)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__Logs__UsuarioId__5165187F");
        });

        modelBuilder.Entity<Paise>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Paises__3214EC072534943F");

            entity.HasIndex(e => e.CodigoIso, "UQ__Paises__F2D697469B06A780").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.CodigoIso)
                .HasMaxLength(3)
                .HasColumnName("CodigoISO");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(100);

            entity.HasOne(d => d.CreadoPor).WithMany(p => p.PaiseCreadoPors)
                .HasForeignKey(d => d.CreadoPorId)
                .HasConstraintName("FK__Paises__CreadoPo__571DF1D5");

            entity.HasOne(d => d.ModificadoPor).WithMany(p => p.PaiseModificadoPors)
                .HasForeignKey(d => d.ModificadoPorId)
                .HasConstraintName("FK__Paises__Modifica__5812160E");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC070A4C094F");

            entity.HasIndex(e => e.Nombre, "UQ__Roles__75E3EFCF0363C244").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion).HasMaxLength(200);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tokens__3214EC07CBAD1B04");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Ip).HasMaxLength(45);
            entity.Property(e => e.Observacion).HasMaxLength(200);
            entity.Property(e => e.Token1)
                .HasMaxLength(1000)
                .HasColumnName("Token");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tokens__UsuarioI__49C3F6B7");
        });

        modelBuilder.Entity<TokensExpirado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TokensEx__3214EC07B8276930");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Ip).HasMaxLength(45);
            entity.Property(e => e.Observacion).HasMaxLength(200);
            entity.Property(e => e.Token).HasMaxLength(1000);

            entity.HasOne(d => d.Usuario).WithMany(p => p.TokensExpirados)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TokensExp__Usuar__4D94879B");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC0702CA7462");

            entity.HasIndex(e => e.NombreUsuario, "UQ__Usuarios__6B0F5AE0818B6422").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Usuarios__A9D1053463094A99").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.Contraseña).HasMaxLength(250);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.NombreUsuario).HasMaxLength(100);

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios__RolId__44FF419A");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
