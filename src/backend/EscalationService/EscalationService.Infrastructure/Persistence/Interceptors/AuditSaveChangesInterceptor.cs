using EscalationService.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EscalationService.Infrastructure.Persistence.Interceptors;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
	private readonly TimeProvider _timeProvider;

	public AuditSaveChangesInterceptor(TimeProvider timeProvider)
	{
		_timeProvider = timeProvider;
	}

	public override InterceptionResult<int> SavingChanges(
		DbContextEventData eventData,
		InterceptionResult<int> result)
	{
		ApplyAuditInfo(eventData.Context);
		return base.SavingChanges(eventData, result);
	}

	public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData,
		InterceptionResult<int> result,
		CancellationToken cancellationToken = new CancellationToken())
	{
		ApplyAuditInfo(eventData.Context);
		return base.SavingChangesAsync(eventData, result, cancellationToken);
	}

	private void ApplyAuditInfo(DbContext? context)
	{
		if (context is null)
		{
			return;
		}

		DateTimeOffset now = _timeProvider.GetUtcNow();

		ApplyCreatedAudit(context, now);
		ApplyUpdatedAudit(context, now);
	}

	private static void ApplyCreatedAudit(DbContext? context, DateTimeOffset now)
	{
		if (context is null)
			return;
		

		var createdEntries = context.ChangeTracker
			.Entries<ICreatableAuditedEntity>()
			.Where(x => x.State is EntityState.Added or EntityState.Modified);

		foreach (var entry in createdEntries)
		{
			if (entry.State == EntityState.Added)
			{
				entry.Entity.CreatedAt = now;
			}
			else if (entry.State == EntityState.Modified)
			{
				// Даже если случайно изменили дату создания, отменяем эти правки чтобы они не попали в БД
				// Дата обновления должна быть иммутабельной и устанавливаться только при создании
				entry.Property(x => x.CreatedAt).IsModified = false;
			}
		}
	}

	private static void ApplyUpdatedAudit(DbContext? context, DateTimeOffset now)
	{
		if (context is null)
		{
			return;
		}
		
		var updatedEntries = context.ChangeTracker
			.Entries<IUpdatableAuditedEntity>()
			.Where(x => x.State is EntityState.Added or EntityState.Modified);

		foreach (var entry in updatedEntries)
		{
			// Устанавливаем дату обновления только в том случае, если мы не установили ее вручную в коде
			// Пример: UserDeviceDeliveryDeal.Close должен установить дату обновления и дату закрытия в одно значение
			// потому что дата закрытия приходит из вне и рассинхрон между закрытием и обновлением нам не нужен
			if (!entry.Property(x => x.UpdatedAt).IsModified)
			{
				entry.Entity.UpdatedAt = now;
			}
		}
	}
}