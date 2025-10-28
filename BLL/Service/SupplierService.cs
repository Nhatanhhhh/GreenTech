using BLL.Service.Interface;
using DAL.DTOs.Supplier;
using DAL.Repositories.Interface;
using DAL.Utils.AutoMapper;

namespace BLL.Service
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierService(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<SupplierDTO> CreateSupplierAsync(CreateSupplierDTO createSupplierDto)
        {
            if (await _supplierRepository.GetByCodeAsync(createSupplierDto.Code) != null)
            {
                throw new ArgumentException($"Nhà cung cấp với mã '{createSupplierDto.Code}' đã tồn tại.");
            }

            var supplier = AutoMapper.ToSupplier(createSupplierDto);
            var createdSupplier = await _supplierRepository.CreateAsync(supplier);
            return AutoMapper.ToSupplierDTO(createdSupplier);
        }

        public async Task DeleteSupplierAsync(int supplierId)
        {
            if (!await _supplierRepository.ExistsAsync(supplierId))
            {
                throw new KeyNotFoundException($"Không tìm thấy nhà cung cấp với ID {supplierId}.");
            }
            // Add business logic here if needed (e.g., check if supplier has associated products)
            await _supplierRepository.DeleteAsync(supplierId);
        }

        public async Task<SupplierDTO> GetSupplierByIdAsync(int supplierId)
        {
            var supplier = await _supplierRepository.GetByIdAsync(supplierId);
            if (supplier == null)
            {
                return null;
            }
            return AutoMapper.ToSupplierDTO(supplier);
        }

        public async Task<IEnumerable<SupplierDTO>> GetSuppliersAsync(SupplierQueryParamsDTO queryParams)
        {
            var suppliers = await _supplierRepository.GetAllAsync(queryParams);
            return AutoMapper.ToSupplierDTOs(suppliers);
        }

        public async Task<int> GetSuppliersCountAsync(SupplierQueryParamsDTO queryParams)
        {
            return await _supplierRepository.CountAsync(queryParams);
        }

        public async Task<SupplierDTO> UpdateSupplierAsync(int supplierId, UpdateSupplierDTO updateSupplierDto)
        {
            var existingSupplier = await _supplierRepository.GetByIdAsync(supplierId);
            if (existingSupplier == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy nhà cung cấp với ID {supplierId}.");
            }

            AutoMapper.ApplyUpdatesToSupplier(updateSupplierDto, existingSupplier);
            await _supplierRepository.UpdateAsync(existingSupplier);
            return AutoMapper.ToSupplierDTO(existingSupplier);
        }
    }
}
